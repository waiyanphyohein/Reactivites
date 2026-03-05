import { Box, Grid, Paper, Typography } from '@mui/material'
import ActivityList from './ActivityList';
import ActivityDetail from '../details/ActivityDetail';
import ActivityForm from '../form/ActivityForm';

type Props = {
  activities: Activity[];
  selectActivity: (id: string) => void;
  cancelSelectActivity: () => void;
  createActivity: (activity: Activity) => void;
  currentUsername: string;
  selectedActivity: Activity | undefined;
}

export default function ActivityDashboard({
  activities,
  selectActivity,
  cancelSelectActivity,
  createActivity,
  currentUsername,
  selectedActivity
}: Props) {
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Paper
        sx={{
          borderRadius: 3,
          p: 3,
          backgroundImage: 'linear-gradient(135deg, #182a73 0%, #218aae 69%, #20a7ac 89%)',
          color: 'white',
        }}
      >
        <Typography variant='h4' fontWeight={700}>Discover Activities</Typography>
        <Typography sx={{ opacity: 0.9 }}>
          Browse upcoming events, explore details, and create your next meetup.
        </Typography>
      </Paper>

      <Grid container spacing={3}>
          <Grid size={7}>
            <ActivityList 
              activities={activities} 
              selectActivity={selectActivity} 
            />
          </Grid>
          <Grid size={5}>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
                {
                  selectedActivity && <ActivityDetail activity={selectedActivity} 
                  cancelSelectActivity={cancelSelectActivity}/>
                }
                <ActivityForm
                  cancelSelectActivity={cancelSelectActivity}
                  currentUsername={currentUsername}
                  onCreateActivity={createActivity}
                />
              </Box>
          </Grid>
      </Grid>
    </Box>
  )
}
