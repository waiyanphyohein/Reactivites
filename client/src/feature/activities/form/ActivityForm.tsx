import { Box, Button, Paper, TextField, Typography } from '@mui/material';
    
type Props = {
  cancelSelectActivity: () => void;
}

export default function ActivityForm({cancelSelectActivity}: Props) {
  return (
    <Paper sx={{borderRadius: 3, boxShadow: 3, padding: 3}}>
        <Typography variant='h5' gutterBottom color='primary'>
            Create Activity
        </Typography>
        <Box component='form' display='flex' flexDirection='column' gap={3} noValidate>
            <TextField label='Title' variant='outlined' fullWidth required />
            <TextField label='Date' type='datetime' variant='outlined' fullWidth required />
            <TextField label='Description' variant='outlined' multiline rows={3} fullWidth required />
            <TextField label='Category' variant='outlined' fullWidth required />
            <TextField label='City' variant='outlined' fullWidth required />
            <TextField label='Venue' variant='outlined' fullWidth required />
            <TextField label='Latitude' variant='outlined' fullWidth required />
            <TextField label='Longitude' variant='outlined' fullWidth required />
            <Box display='flex' justifyContent='end' gap={2}>
                <Button type='button' variant='contained' color='inherit' onClick={cancelSelectActivity}>Cancel</Button>
                <Button type='submit' variant='contained' color='success'>Submit</Button>            
            </Box>
        </Box>
    </Paper>
  )
}
