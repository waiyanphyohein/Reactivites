import { useEffect, useState } from 'react';
import './App.css'
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
    <div>
      <h3 className='app' style={{ color: "red" }}> {title} </h3>
      <ul>
        {activities.map(activity => (
          <li key={activity.id}>{activity.title}</li>
        ))}
      </ul>
    </div>
  )
}

export default App
