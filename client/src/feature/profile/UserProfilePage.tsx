import { useEffect, useMemo, useState } from 'react';
import {
  Avatar,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Divider,
  Stack,
  Tab,
  Tabs,
  Typography,
} from '@mui/material';


type Props = {
  profile: UserProfile;
}

export default function UserProfilePage({ profile }: Props) {
  const initialTab: 'past' | 'future' =
    profile.futureEvents.length > 0 ? 'future' : 'past';
  const [activeTab, setActiveTab] = useState<'past' | 'future'>(initialTab);
  const [showCreatedOnly, setShowCreatedOnly] = useState(false);

  useEffect(() => {
    if (profile.futureEvents.length === 0 && profile.pastEvents.length > 0 && activeTab === 'future') {
      setActiveTab('past');
    }

    if (profile.futureEvents.length > 0 && profile.pastEvents.length === 0 && activeTab === 'past') {
      setActiveTab('future');
    }
  }, [activeTab, profile.futureEvents.length, profile.pastEvents.length]);

  const visibleEvents = useMemo(() => {
    const baseEvents = activeTab === 'future' ? profile.futureEvents : profile.pastEvents;
    if (!showCreatedOnly) return baseEvents;

    return baseEvents.filter(
      eventItem =>
        (eventItem.creatorDisplayName ?? '').trim().toLowerCase() ===
        profile.displayName.trim().toLowerCase()
    );
  }, [activeTab, profile.futureEvents, profile.pastEvents, showCreatedOnly, profile.displayName]);

  return (
    <Stack spacing={3} sx={{ pt: 0 }}>
      <Card sx={{ borderRadius: 3, boxShadow: 3, overflow: 'hidden' }}>
        <Box
          sx={{
            height: 140,
            backgroundImage: 'linear-gradient(135deg, #182a73 0%, #218aae 69%, #20a7ac 89%)',
          }}
        />
        <CardContent sx={{ pt: 2, pb: 3 }}>
          <Stack
            direction={{ xs: 'column', sm: 'row' }}
            spacing={2}
            alignItems={{ xs: 'flex-start', sm: 'flex-end' }}
          >
            <Avatar
              src={profile.avatarUrl}
              alt={profile.displayName}
              sx={{ width: 96, height: 96, mt: -8, border: '4px solid white' }}
            />
            <Box sx={{ pb: 0.5 }}>
              <Typography variant='h4' fontWeight='bold' sx={{ lineHeight: 1.1 }}>
                {profile.displayName}
              </Typography>
              <Typography variant='body1' color='text.secondary'>
                @{profile.username}
              </Typography>
            </Box>
          </Stack>
        </CardContent>
      </Card>

      <Card sx={{ borderRadius: 3, boxShadow: 3 }}>
        <Tabs
          value={activeTab}
          onChange={(_, value: 'past' | 'future') => setActiveTab(value)}
          sx={{ px: 2 }}
        >
          <Tab value='future' label={`Future Events (${profile.futureEvents.length})`} />
          <Tab value='past' label={`Past Events (${profile.pastEvents.length})`} />
        </Tabs>
        <Divider />
        <Stack direction='row' spacing={1} sx={{ px: 2, pt: 2 }}>
          <Button
            size='small'
            variant={showCreatedOnly ? 'outlined' : 'contained'}
            onClick={() => setShowCreatedOnly(false)}
          >
            All
          </Button>
          <Button
            size='small'
            variant={showCreatedOnly ? 'contained' : 'outlined'}
            onClick={() => setShowCreatedOnly(true)}
          >
            Created by me
          </Button>
        </Stack>
        <Stack spacing={2} sx={{ p: 2 }}>
          {visibleEvents.length === 0 && (
            <Typography color='text.secondary'>No events available in this tab.</Typography>
          )}

          {visibleEvents.map((eventItem) => (
            <Card key={eventItem.id} variant='outlined' sx={{ borderRadius: 2 }}>
              <CardContent>
                <Stack spacing={1}>
                  <Stack direction='row' alignItems='center' justifyContent='space-between'>
                    <Typography variant='h6'>{eventItem.title}</Typography>
                    {eventItem.category && <Chip label={eventItem.category} variant='outlined' />}
                  </Stack>
                  <Typography color='text.secondary'>
                    {new Date(eventItem.date).toLocaleString()}
                  </Typography>
                  <Typography variant='body2'>{eventItem.description}</Typography>
                  <Typography variant='subtitle2'>{eventItem.city} / {eventItem.venue}</Typography>
                </Stack>
              </CardContent>
            </Card>
          ))}
        </Stack>
      </Card>
    </Stack>
  );
}
