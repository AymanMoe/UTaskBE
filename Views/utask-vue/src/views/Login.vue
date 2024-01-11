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
                            <v-checkbox v-model="rememberMe" label="Remember Me"></v-checkbox>
                            <v-btn type="submit" color="primary">Submit</v-btn>
                        </v-form>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>
    </v-container>
</template>

<script>
export default {
    data() {
        return {
            email: '',
            password: '',
            rememberMe: false
        }
    },
    methods: {
        submitForm() {
            // Call your API here
            // Example: axios.post('/api/login', { email: this.email, password: this.password, rememberMe: this.rememberMe })
            //   .then(response => {
            //     // Handle response
            //   })
            //   .catch(error => {
            //     // Handle error
            //   })
        }
    }
}
</script>
