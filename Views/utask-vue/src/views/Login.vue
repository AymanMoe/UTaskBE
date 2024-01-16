<template>
    <v-container fluid fill-height>
        <v-row align="center" justify="center">
            <v-col cols="12" sm="8" md="4">
                <v-card>
                    <v-card-title class="text-center">Login</v-card-title>
                    <v-card-text>
                        <v-form @submit="submitForm">
                            <v-text-field v-model="email" label="Email"></v-text-field>
                            <v-text-field v-model="password" label="Password" type="password"></v-text-field>
                            <router-link :to="{ path: '/register' }"><button>Not a user? Register instead!</button></router-link>
                            <v-checkbox v-model="rememberMe" label="Remember Me"></v-checkbox>
                            <v-btn type="submit" color="primary">Submit</v-btn>
                        </v-form>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>
    </v-container>
 
    <div v-if="errorMessage" class="error-message" style="color: red;">{{ errorMessage }}</div>

</template>

<script>
import axios from 'axios';

export default {
    data() {
        return {
            email: '',
            password: '',
            rememberMe: '',
            errorMessage: ''
        }
    },
    methods: {
        submitForm() {
            axios.post('http://localhost:5204/api/auth/login', { email: this.email, password: this.password, rememberMe: this.rememberMe })
            .then(res => console.log(res))
            .catch(error => {
                    console.log("Unauthorized");              
                    console.error(error);
                    this.errorMessage = 'Please log in with valid credentials.';
            });
        }
    }
}
</script>


