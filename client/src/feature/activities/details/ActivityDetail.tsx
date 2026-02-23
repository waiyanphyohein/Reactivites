import { Card, Typography, CardContent, CardMedia, Button, CardActions } from '@mui/material';

type Props = {
  activity: Activity;
  cancelSelectActivity: () => void;
}

export default function ActivityDetail({activity, cancelSelectActivity}: Props) {
  return (
    <Card sx={{ borderRadius: 3, boxShadow: 3, p: 2 }}>
        <CardMedia component='img' image={`/images/categoryImages/${activity.category.toLowerCase().replace(/ /g, '')}.jpg`} alt={activity.title} />
        <CardContent>
            <Typography variant='h5'>{activity.title}</Typography>
            <Typography sx={{color: 'text.secondary'}}>{new Date(activity.date).toLocaleDateString()}</Typography>
            <Typography variant='body1'>{activity.description}</Typography>
        </CardContent>
        <CardActions sx={{display: 'flex', justifyContent: 'space-between', pb: 2}}>
            <Button size='medium' variant='contained' color='primary'>Edit</Button>
            <Button size='medium' variant='contained' color='inherit' onClick={cancelSelectActivity}>Cancel</Button>
        </CardActions>
    </Card>
  )
}