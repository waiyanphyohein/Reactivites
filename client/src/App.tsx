import { Fragment, useEffect, useState } from 'react';
import './App.css'
import Typography from '@mui/material/Typography';
import { List, ListItem, ListItemText } from '@mui/material';
import axios from 'axios';
function App() {

  const title = 'Reactivities'
  const [activities, setActivities] = useState<Activity[]>([]);
  
  useEffect(() => {
    axios.get('https://localhost:5001/api/activities/')
      .then(response => setActivities(response.data))
      .catch(error => console.error('Error fetching activities:', error));
  }, []);

  return (
    <Fragment>
      <Typography variant='h3'> {title} </Typography>
      <List>
        {activities.map(activity => (
          <ListItem key={activity.id}>
            <ListItemText primary={activity.title} /> 
          </ListItem>
        ))}
      </List>
    </Fragment>
  )
}

export default App
