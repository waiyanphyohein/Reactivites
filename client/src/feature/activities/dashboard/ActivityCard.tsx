import { Button, Card, CardActions, CardContent, Chip, Typography } from '@mui/material';

type Props = {
  activity: Activity;
}

export default function ActivityCard({activity}: Props) {
  return (
    <Card sx={{ borderRadius: 3, boxShadow: 3, p: 2 }}>
        <CardContent>
            <Typography variant='h5'>{activity.title}</Typography>
            <Typography sx={{color: 'text.secondary'}}>{new Date(activity.date).toLocaleDateString()}</Typography>
            <Typography variant='body2'>{activity.description}</Typography>
            <Typography variant='subtitle1'>{activity.city} / {activity.venue}</Typography>            
        </CardContent>
        <CardActions sx={{display: 'flex', justifyContent: 'space-between', pb: 2}}>
            <Chip label={activity.category} variant='outlined'/>
            <Button size='medium' variant='contained'>View</Button>
        </CardActions>
    </Card>
  )
}
