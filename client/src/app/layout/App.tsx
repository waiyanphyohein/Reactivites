import { useEffect, useState } from 'react';
import './styles.css';
import axios from 'axios';
import Navbar from './Navbar';
import { Box, Container, CssBaseline } from '@mui/material';
import ActivityDashboard from '../../feature/activities/dashboard/ActivityDashboard';
import { getApiBaseUrl } from '../../lib/api';

function App() {

  const [activities, setActivities] = useState<Activity[]>([]);
  const [selectedActivity, setSelectedActivity] = useState<Activity | undefined>(undefined);

  useEffect(() => {
    axios.get(`${getApiBaseUrl()}/api/activities/`)
      .then(response => setActivities(response.data))
      .catch(error => console.error('Error fetching activities:', error));
  }, []);

  const handleSelectActivity = (id: string) => {
    setSelectedActivity(activities.find(activity => activity.id === id));
  }

  const handleCancelSelectActivity = () => {
    setSelectedActivity(undefined);
  }

  return (
    <Box sx={{ bgcolor: 'grey.100', minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <CssBaseline />
      <Navbar />
      <Container maxWidth="xl" sx={{ mt: 3 }}>        
        <ActivityDashboard 
        activities={activities}
        selectActivity={handleSelectActivity}        
        cancelSelectActivity={handleCancelSelectActivity}
        selectedActivity={selectedActivity}
        />
      </Container>
    </Box>
  )
}

export default App
