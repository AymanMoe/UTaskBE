const express = require('express');
const bodyParser = require('body-parser');

const app = express();
const port = process.env.PORT || 5204;
app.use(bodyParser.json());

const registeredUsers = [];

app.post('/api/auth/register', (req, res) => {
  const registrationData = req.body;

  const newUser = {
    firstName: registrationData.firstName,
    lastName: registrationData.lastName,
    email: registrationData.email,
    password: registrationData.password,
  };

  // Check if the email is already registered
  if (registeredUsers.some(user => user.email === newUser.email)) {
    return res.status(400).json({ message: 'Email is already registered' });
  }

  // Add the user to the list of registered users
  registeredUsers.push(newUser);

  console.log('User registered:', newUser);
  res.status(201).json({ message: 'User registered successfully' });
});

app.listen(port, () => {
  console.log(`Server is running on http://localhost:5204/index.html`);
});
