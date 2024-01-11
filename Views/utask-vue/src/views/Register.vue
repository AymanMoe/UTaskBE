<template>
    <v-container fluid>
        <v-row justify="center">
            <v-col cols="12" sm="8" md="6">
                <v-card>
                    <v-card-title class="text-center">Registration</v-card-title>
                    <v-card-text>
                        <v-stepper v-model="activeStep" :alt-labels="true">
                            <v-stepper-header>
                                <v-stepper-step
                                    v-for="(step, index) in steps"
                                    :key="index"
                                    :complete="index < activeStep"
                                >
                                    {{ step }}
                                </v-stepper-step>
                            </v-stepper-header>

                            <v-stepper-items>
                                <v-stepper-content step="1">
                                    <v-text-field v-model="firstName" label="First Name"></v-text-field>
                                    <v-text-field v-model="lastName" label="Last Name"></v-text-field>
                                    <v-text-field v-model="email" label="Email"></v-text-field>
                                    <v-text-field v-model="password" label="Password" type="password"></v-text-field>
                                </v-stepper-content>

                                <v-stepper-content step="2">
                                    <v-radio-group v-model="type" row>
                                        <v-radio label="Client" value="Client"></v-radio>
                                        <v-radio label="Provider" value="Provider"></v-radio>
                                    </v-radio-group>
                                </v-stepper-content>

                                <v-stepper-content step="3">
                                    <v-text-field v-model="address.streetAddress" label="Street Address"></v-text-field>
                                    <v-text-field v-model="address.city" label="City"></v-text-field>
                                    <v-text-field v-model="address.postalCode" label="Postal Code"></v-text-field>
                                    <v-text-field v-model="address.country" label="Country"></v-text-field>
                                </v-stepper-content>
                            </v-stepper-items>
                        </v-stepper>
                    </v-card-text>
                    <v-card-actions>
                        <v-btn @click="submitForm" color="primary">Submit</v-btn>
                    </v-card-actions>
                </v-card>
            </v-col>
        </v-row>
    </v-container>
</template>

<script>
export default {
    data() {
        return {
            activeStep: 1,
            steps: ["Step 1", "Step 2", "Step 3"],
            firstName: "",
            lastName: "",
            email: "",
            password: "",
            type: "",
            address: {
                streetAddress: "",
                city: "",
                postalCode: "",
                country: ""
            }
        };
    },
    methods: {
        submitForm() {
            // Call your API here to submit the form data
            // You can access the form data using the data properties defined above
        }
    }
};
</script>

