import { Card, Typography, CardContent, CardMedia, Button, CardActions } from '@mui/material';

type Props = {
  activity: Activity;
}

export default function ActivityDetail({activity}: Props) {
  return (
    <Card sx={{ borderRadius: 3, boxShadow: 3, p: 2 }}>
        <CardMedia component='img' image={`/images/categoryImages/${activity.category.toLowerCase()}.jpg`} alt={activity.title} />
        <CardContent>
            <Typography variant='h5'>{activity.title}</Typography>
            <Typography sx={{color: 'text.secondary'}}>{new Date(activity.date).toLocaleDateString()}</Typography>
            <Typography variant='body1'>{activity.description}</Typography>
        </CardContent>
        <CardActions sx={{display: 'flex', justifyContent: 'space-between', pb: 2}}>
            <Button size='medium' variant='contained' color='primary'>Edit</Button>
            <Button size='medium' variant='contained' color='inherit'>Cancel</Button>
        </CardActions>
    </Card>
  )
}