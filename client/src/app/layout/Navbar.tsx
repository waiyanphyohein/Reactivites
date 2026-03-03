import { type MouseEvent, useState } from 'react';
import { Avatar, Box, Button, Container, Menu, MenuItem, Stack, Typography } from '@mui/material';
import { Groups } from '@mui/icons-material';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';

type Props = {
    activeView: 'activities' | 'profile';
    onChangeView: (view: 'activities' | 'profile') => void;
    onCreateActivityClick: () => void;
    onLogout: () => void;
    username: string;
    avatarUrl: string;
}

export default function Navbar({
    activeView,
    onChangeView,
    onCreateActivityClick,
    onLogout,
    username,
    avatarUrl
}: Props) {
    const [menuAnchorEl, setMenuAnchorEl] = useState<null | HTMLElement>(null);
    const isMenuOpen = Boolean(menuAnchorEl);

    const handleOpenMenu = (event: MouseEvent<HTMLElement>) => {
        setMenuAnchorEl(event.currentTarget);
    };

    const handleCloseMenu = () => {
        setMenuAnchorEl(null);
    };

    return (
        <Box>
            <AppBar position='static' sx={{ backgroundImage: 'linear-gradient(135deg, #182a73 0%, #218aae 69%, #20a7ac 89%)' }}>
                <Container maxWidth='xl'>
                    <Toolbar sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <Box>
                            <MenuItem sx={{ display: 'flex', gap: 2 }} onClick={() => onChangeView('activities')}>
                                <Groups fontSize='large' />
                                <Typography variant='h4' fontWeight='bold' component='div' sx={{ flexGrow: 1 }}>
                                    Reactivities
                                </Typography>
                            </MenuItem>
                        </Box>

                        <Box sx={{ display: 'flex' }}>
                            <MenuItem
                                sx={{ fontSize: '1.2rem', textTransform: 'uppercase', fontWeight: 'bold', opacity: activeView === 'activities' ? 1 : 0.8 }}
                                onClick={() => onChangeView('activities')}
                            >
                                Activities
                            </MenuItem>
                        </Box>

                        <Stack direction='row' alignItems='center' spacing={2}>
                            <MenuItem
                                sx={{ display: 'flex', gap: 1 }}
                                onClick={handleOpenMenu}
                                aria-haspopup='true'
                                aria-expanded={isMenuOpen ? 'true' : undefined}
                            >
                                <Avatar src={avatarUrl} alt={username} />
                                <Typography fontWeight='bold'>{username}</Typography>
                            </MenuItem>
                            <Menu
                                anchorEl={menuAnchorEl}
                                open={isMenuOpen}
                                onClose={handleCloseMenu}
                            >
                                <MenuItem
                                    onClick={() => {
                                        onChangeView('profile');
                                        handleCloseMenu();
                                    }}
                                >
                                    Profile
                                </MenuItem>
                                <MenuItem
                                    onClick={() => {
                                        onLogout();
                                        handleCloseMenu();
                                    }}
                                >
                                    Logout
                                </MenuItem>
                            </Menu>
                            <Button size='large' color='warning' variant='contained' onClick={onCreateActivityClick}>
                                Create Activity
                            </Button>
                        </Stack>
                    </Toolbar>
                </Container>
            </AppBar>
        </Box>
    );
}
