import { Fragment, useEffect, useState } from 'react';
import './styles.css';
import axios from 'axios';
import Navbar from './Navbar';
import { Container, CssBaseline } from '@mui/material';
import ActivityDashboard from '../../feature/activities/activitydashboard';

function App() {

  const [activities, setActivities] = useState<Activity[]>([]);
  
  useEffect(() => {
    axios.get('https://localhost:5001/api/activities/')
      .then(response => setActivities(response.data))
      .catch(error => console.error('Error fetching activities:', error));
  }, []);

  return (
    <Fragment>
      <CssBaseline />
      <Navbar />
      <Container maxWidth="xl" sx={{ mt: 3 }}>        
        {/* <Typography variant='h3'> {title} </Typography> */}
        <ActivityDashboard activities={activities}/>
      </Container>
    </Fragment>
  )
}

export default App
