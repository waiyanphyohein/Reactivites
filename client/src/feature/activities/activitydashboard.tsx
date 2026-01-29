import { Grid, ListItem, List, ListItemText } from '@mui/material'

type Props = {
  activities: Activity[];
}

export default function ActivityDashboard({activities}: Props) {
  return (
    <Grid container>
        <Grid size={9}>
        <List>
          {activities.map((activity: Activity, numbering: number) => (
            <ListItem key={activity.id}>
              <ListItemText primary={  `${numbering}: ${activity.title} (Date: ${new Date(activity.date).toLocaleDateString()})` } /> 
            </ListItem>
          ))}
        </List>
        </Grid>
    </Grid>
  )
}
