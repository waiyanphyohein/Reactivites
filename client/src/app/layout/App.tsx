import { useEffect, useState } from 'react';
import './styles.css';
import axios from 'axios';
import Navbar from './Navbar';
import { Box, Container, CssBaseline } from '@mui/material';
import ActivityDashboard from '../../feature/activities/dashboard/ActivityDashboard';
import LoginPage from '../../feature/auth/LoginPage';
import UserProfilePage from '../../feature/profile/UserProfilePage';
import { getApiBaseUrl } from '../../lib/api';

const fallbackProfile: UserProfile = {
  username: 'jeff',
  displayName: 'Jeff',
  avatarUrl: '/images/jeff-placeholder.svg',
  pastEvents: [],
  futureEvents: [],
};

function normalizeActivity(raw: Record<string, unknown>): Activity | null {
  const id = (raw.id ?? raw.Id) as string | undefined;
  const title = (raw.title ?? raw.Title) as string | undefined;
  const date = (raw.date ?? raw.Date) as string | undefined;
  const city = (raw.city ?? raw.City) as string | undefined;
  const venue = (raw.venue ?? raw.Venue) as string | undefined;

  if (!id || !title || !date || !city || !venue) return null;

  return {
    id,
    title,
    date,
    city,
    venue,
    description: ((raw.description ?? raw.Description) as string | undefined) ?? '',
    category: ((raw.category ?? raw.Category) as string | undefined) ?? 'General',
    latitude: Number((raw.latitude ?? raw.Latitude) as number | undefined),
    longitude: Number((raw.longitude ?? raw.Longitude) as number | undefined),
    creatorDisplayName: (raw.creatorDisplayName ?? raw.CreatorDisplayName) as string | undefined,
  };
}

function profileEventToActivity(eventItem: ProfileEvent): Activity {
  return {
    id: eventItem.id,
    title: eventItem.title,
    date: eventItem.date,
    description: eventItem.description ?? '',
    category: eventItem.category ?? 'General',
    city: eventItem.city,
    venue: eventItem.venue,
    latitude: 0,
    longitude: 0,
    creatorDisplayName: eventItem.creatorDisplayName,
  };
}

function toProfileEvent(activity: Activity): ProfileEvent {
  return {
    id: activity.id,
    title: activity.title,
    date: activity.date,
    description: activity.description,
    category: activity.category,
    city: activity.city,
    venue: activity.venue,
    creatorDisplayName: activity.creatorDisplayName,
  };
}

function buildProfileFromActivities(activities: Activity[]): UserProfile {
  const now = new Date();
  const mapped = activities.map(toProfileEvent);

  return {
    ...fallbackProfile,
    futureEvents: mapped.filter(eventItem => new Date(eventItem.date) >= now),
    pastEvents: mapped.filter(eventItem => new Date(eventItem.date) < now),
  };
}

function App() {
  const isTestMode = import.meta.env.MODE === 'test';
  const storage =
    typeof window !== 'undefined' &&
    window.localStorage &&
    typeof window.localStorage.getItem === 'function' &&
    typeof window.localStorage.setItem === 'function'
      ? window.localStorage
      : null;
  const persistedAuth = storage?.getItem('reactivities_auth') === '1';
  const persistedUsername = storage?.getItem('reactivities_username');

  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(isTestMode || persistedAuth);
  const [activities, setActivities] = useState<Activity[]>([]);
  const [selectedActivity, setSelectedActivity] = useState<Activity | undefined>(undefined);
  const [activeView, setActiveView] = useState<'activities' | 'profile'>('activities');
  const [profile, setProfile] = useState<UserProfile>({
    ...fallbackProfile,
    displayName: persistedUsername ?? fallbackProfile.displayName,
  });

  useEffect(() => {
    if (!isAuthenticated) return;

    const loadData = async () => {
      let loadedActivities: Activity[] = [];

      try {
        const activitiesResponse = await axios.get(`${getApiBaseUrl()}/api/activities/`);
        loadedActivities = Array.isArray(activitiesResponse.data)
          ? activitiesResponse.data
              .map((item: unknown) => normalizeActivity((item as Record<string, unknown>)))
              .filter((item: Activity | null): item is Activity => item !== null)
          : [];
        setActivities(loadedActivities);
      } catch (error) {
        console.error('Error fetching activities:', error);
      }

      const profileFromActivities = buildProfileFromActivities(loadedActivities);

      try {
        const profileResponse = await axios.get(`${getApiBaseUrl()}/api/profiles/jeff`);
        const apiProfile = profileResponse.data as UserProfile;
        const hasProfileEvents =
          apiProfile.futureEvents.length > 0 || apiProfile.pastEvents.length > 0;

        if (loadedActivities.length === 0 && hasProfileEvents) {
          setActivities(
            [...apiProfile.futureEvents, ...apiProfile.pastEvents].map(profileEventToActivity)
          );
        }

        setProfile(hasProfileEvents ? apiProfile : profileFromActivities);
      } catch (error) {
        console.error('Error fetching profile:', error);
        setProfile(profileFromActivities);
      }
    };

    void loadData();
  }, [isAuthenticated]);

  const handleSelectActivity = (id: string) => {
    setSelectedActivity(activities.find(activity => activity.id === id));
  };

  const handleCancelSelectActivity = () => {
    setSelectedActivity(undefined);
  };

  const handleCreateActivity = (activity: Activity) => {
    setActivities(current => [activity, ...current]);

    const currentUsername = profile.displayName.trim().toLowerCase();
    const activityCreator = (activity.creatorDisplayName ?? '').trim().toLowerCase();
    if (activityCreator !== currentUsername) return;

    const profileEvent = toProfileEvent(activity);
    const isFuture = new Date(activity.date) >= new Date();

    setProfile(current => ({
      ...current,
      futureEvents: isFuture ? [profileEvent, ...current.futureEvents] : current.futureEvents,
      pastEvents: isFuture ? current.pastEvents : [profileEvent, ...current.pastEvents],
    }));
  };

  const handleCreateActivityAction = () => {
    setActiveView('activities');
    setSelectedActivity(undefined);

    if (typeof window !== 'undefined') {
      window.requestAnimationFrame(() => {
        const element = document.getElementById('create-activity-form');
        if (element && typeof element.scrollIntoView === 'function') {
          element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
      });
    }
  };

  const handleLogin = (username: string) => {
    if (storage) {
      storage.setItem('reactivities_auth', '1');
      storage.setItem('reactivities_username', username);
    }

    setProfile(current => ({ ...current, displayName: username }));
    setIsAuthenticated(true);
  };

  const handleLogout = () => {
    if (storage) {
      storage.removeItem('reactivities_auth');
      storage.removeItem('reactivities_username');
    }

    setIsAuthenticated(false);
    setActiveView('activities');
    setSelectedActivity(undefined);
  };

  if (!isAuthenticated) {
    return (
      <>
        <CssBaseline />
        <LoginPage onLogin={handleLogin} />
      </>
    );
  }

  return (
    <Box sx={{ bgcolor: 'grey.100', minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <CssBaseline />
      <Navbar
        activeView={activeView}
        onChangeView={setActiveView}
        onCreateActivityClick={handleCreateActivityAction}
        onLogout={handleLogout}
        username={profile.displayName}
        avatarUrl={profile.avatarUrl}
      />
      <Container
        maxWidth='xl'
        sx={{
          mt: activeView === 'profile' ? 2 : 3,
          px: activeView === 'profile' ? { xs: 2, sm: 3 } : undefined,
          pb: activeView === 'profile' ? 3 : undefined,
        }}
      >
        {activeView === 'activities' && (
          <ActivityDashboard
            activities={activities}
            selectActivity={handleSelectActivity}
            cancelSelectActivity={handleCancelSelectActivity}
            createActivity={handleCreateActivity}
            currentUsername={profile.displayName}
            selectedActivity={selectedActivity}
          />
        )}

        {activeView === 'profile' && <UserProfilePage profile={profile} />}
      </Container>
    </Box>
  );
}

export default App;
