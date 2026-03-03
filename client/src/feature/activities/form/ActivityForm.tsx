import { useState } from 'react';
import { Box, Button, Paper, TextField, Typography } from '@mui/material';
    
type Props = {
  cancelSelectActivity: () => void;
  currentUsername: string;
  onCreateActivity: (activity: Activity) => void;
}

type FormState = {
  title: string;
  date: string;
  description: string;
  category: string;
  city: string;
  venue: string;
  latitude: string;
  longitude: string;
};

const initialState: FormState = {
  title: '',
  date: '',
  description: '',
  category: '',
  city: '',
  venue: '',
  latitude: '',
  longitude: '',
};

export default function ActivityForm({cancelSelectActivity, currentUsername, onCreateActivity}: Props) {
  const [formState, setFormState] = useState<FormState>(initialState);

  const handleFieldChange = (field: keyof FormState, value: string) => {
    setFormState(current => ({ ...current, [field]: value }));
  };

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!formState.title || !formState.date || !formState.city || !formState.venue) return;

    const generatedId =
      typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function'
        ? crypto.randomUUID()
        : `${Date.now()}`;

    onCreateActivity({
      id: generatedId,
      title: formState.title,
      date: formState.date,
      description: formState.description,
      category: formState.category || 'General',
      city: formState.city,
      venue: formState.venue,
      latitude: Number(formState.latitude) || 0,
      longitude: Number(formState.longitude) || 0,
      creatorDisplayName: currentUsername,
    });

    setFormState(initialState);
  };

  return (
    <Paper id='create-activity-form' sx={{borderRadius: 3, boxShadow: 3, padding: 3}}>
        <Typography variant='h5' gutterBottom color='primary'>
            Create Activity
        </Typography>
        <Typography variant='body2' sx={{ mb: 2 }} color='text.secondary'>
            Creator: {currentUsername}
        </Typography>
        <Box component='form' display='flex' flexDirection='column' gap={3} noValidate onSubmit={handleSubmit}>
            <TextField label='Title' variant='outlined' fullWidth required value={formState.title} onChange={(e) => handleFieldChange('title', e.target.value)} />
            <TextField label='Date' type='datetime-local' variant='outlined' fullWidth required value={formState.date} onChange={(e) => handleFieldChange('date', e.target.value)} InputLabelProps={{ shrink: true }} />
            <TextField label='Description' variant='outlined' multiline rows={3} fullWidth required value={formState.description} onChange={(e) => handleFieldChange('description', e.target.value)} />
            <TextField label='Category' variant='outlined' fullWidth required value={formState.category} onChange={(e) => handleFieldChange('category', e.target.value)} />
            <TextField label='City' variant='outlined' fullWidth required value={formState.city} onChange={(e) => handleFieldChange('city', e.target.value)} />
            <TextField label='Venue' variant='outlined' fullWidth required value={formState.venue} onChange={(e) => handleFieldChange('venue', e.target.value)} />
            <TextField label='Latitude' variant='outlined' fullWidth required value={formState.latitude} onChange={(e) => handleFieldChange('latitude', e.target.value)} />
            <TextField label='Longitude' variant='outlined' fullWidth required value={formState.longitude} onChange={(e) => handleFieldChange('longitude', e.target.value)} />
            <Box display='flex' justifyContent='end' gap={2}>
                <Button type='button' variant='contained' color='inherit' onClick={cancelSelectActivity}>Cancel</Button>
                <Button type='submit' variant='contained' color='success'>Submit</Button>            
            </Box>
        </Box>
    </Paper>
  )
}
