import { type FormEvent, useState } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Divider,
  Stack,
  TextField,
  Typography,
} from '@mui/material';

type Props = {
  onLogin: (username: string) => void;
};

export default function LoginPage({ onLogin }: Props) {
  const [email, setEmail] = useState('jeff@reactivities.app');
  const [password, setPassword] = useState('');

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault();
    const usernamePart = email.split('@')[0]?.trim();
    const username = usernamePart ? usernamePart : 'jeff';
    onLogin(username.charAt(0).toUpperCase() + username.slice(1));
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'grid',
        placeItems: 'center',
        background: 'linear-gradient(180deg, #f7f9fc 0%, #ebf1f7 100%)',
        px: 2,
      }}
    >
      <Card sx={{ width: '100%', maxWidth: 980, borderRadius: 4, boxShadow: 6 }}>
        <CardContent sx={{ p: { xs: 3, md: 4 } }}>
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', md: '1.1fr 1fr' },
              gap: 4,
              alignItems: 'center',
            }}
          >
            <Box>
              <Typography variant='h3' fontWeight={800} sx={{ mb: 1 }}>
                Welcome back
              </Typography>
              <Typography variant='h6' color='text.secondary' sx={{ mb: 3 }}>
                Sign in to manage your activities, RSVPs, and upcoming meetups.
              </Typography>
              <Stack spacing={1.5}>
                <Typography color='text.secondary'>Discover events near you</Typography>
                <Typography color='text.secondary'>Track past and future activities</Typography>
                <Typography color='text.secondary'>Keep your profile updated in one place</Typography>
              </Stack>
            </Box>

            <Box component='form' onSubmit={handleSubmit}>
              <Stack spacing={2}>
                <Typography variant='h5' fontWeight={700}>
                  Log in
                </Typography>
                <TextField
                  type='email'
                  label='Email'
                  fullWidth
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  required
                />
                <TextField
                  type='password'
                  label='Password'
                  fullWidth
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                  required
                />
                <Button size='large' type='submit' variant='contained'>
                  Continue
                </Button>

                <Divider>or</Divider>

                <Button size='large' variant='outlined'>
                  Continue with Google
                </Button>
                <Button size='large' variant='outlined'>
                  Continue with Apple
                </Button>

                <Typography variant='body2' color='text.secondary' textAlign='center'>
                  New to Reactivities? Sign up in seconds.
                </Typography>
              </Stack>
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
}
