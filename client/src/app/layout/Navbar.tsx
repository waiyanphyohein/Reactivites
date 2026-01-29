import { Groups } from "@mui/icons-material";
import AppBar from "@mui/material/AppBar";
import Toolbar from "@mui/material/Toolbar";
import Typography from "@mui/material/Typography";
import Button from "@mui/material/Button";
import Box from "@mui/material/Box";
import Container from "@mui/material/Container";
import { MenuItem } from "@mui/material";

export default function Navbar() {
    return (
        <Box sx={{ flexGrow: 1 }}>
        <AppBar position="static" sx={{ backgroundImage: 'linear-gradient(135deg, #182a73 0%, #218aae 69%, #20a7ac 89%)' }}>
            <Container maxWidth="xl">
            <Toolbar sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Box>
                    <MenuItem sx={{display: 'flex', gap: 2}}>
                        <Groups fontSize="large" />
                        <Typography variant="h4" fontWeight="bold" component="div" sx={{ flexGrow: 1 }}>
                            Reactivities
                        </Typography>
                    </MenuItem>
                </Box>
                <Box sx={{display: 'flex'}}>
                    <MenuItem sx={{fontSize: '1.2rem', textTransform: 'uppercase', fontWeight: 'bold'}}>
                        Activities
                    </MenuItem>
                    <MenuItem sx={{fontSize: '1.2rem', textTransform: 'uppercase', fontWeight: 'bold'}}>
                        About
                    </MenuItem>
                    <MenuItem sx={{fontSize: '1.2rem', textTransform: 'uppercase', fontWeight: 'bold'}}>
                        Contact
                    </MenuItem>
                </Box>
                <Button size="large" color="warning" variant="contained">Create Activity</Button>
            </Toolbar>
            </Container>            
        </AppBar>
        </Box>
    )
}