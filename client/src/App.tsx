import { Fragment, useEffect, useState } from 'react';
import './App.css'
import Typography from '@mui/material/Typography';
import { List, ListItem, ListItemText } from '@mui/material';
function App() {

  const title = 'Reactivities'
  const [activities, setActivities] = useState<Activity[]>([]);
  
  useEffect(() => {
    fetch('https://localhost:5001/api/activities/')
      .then(response => response.json())
      .then(data => setActivities(data))
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
