import { useEffect, useState } from 'react';
import './styles.css';
import axios from 'axios';
import Navbar from './Navbar';
import { Box, Container, CssBaseline } from '@mui/material';
import ActivityDashboard from '../../feature/activities/dashboard/ActivityDashboard';

function App() {

  const [activities, setActivities] = useState<Activity[]>([]);
  
  useEffect(() => {
    axios.get('https://localhost:5001/api/activities/')
      .then(response => setActivities(response.data))
      .catch(error => console.error('Error fetching activities:', error));
  }, []);

  return (
    <Box sx={{ bgcolor: 'grey.100', minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <CssBaseline />
      <Navbar />
      <Container maxWidth="xl" sx={{ mt: 3 }}>        
        {/* <Typography variant='h3'> {title} </Typography> */}
        <ActivityDashboard activities={activities}/>
      </Container>
    </Box>
  )
}

export default App
