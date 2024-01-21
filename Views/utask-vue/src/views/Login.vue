<script>
import axios from 'axios';
import authThemeMask from '../../src/assets/Images/Pages/Login/LoginBackground.png';
import petplay from '../../src/assets/Images/Pages/Login/petplay.png';
import pick from '../../src/assets/Images/Pages/Login/pick.png';
import Logopng from '../../src/assets/Images/Logo/Logo Blue.png';
import { useTheme } from 'vuetify'


export default {
    data() {
        return {
            Logo: "<img src=" + Logopng + " alt='Logo'  style='width: 50%; height: auto;'>",
            email: '',
            password: '',
            rememberMe: false,
            errorMessage: ''
        }
    },
    methods: {
        submitForm() {
            console.log("submitting form");
            console.log(this.email);
            console.log(this.rememberMe);
            axios.post('https://utaskapi.azurewebsites.net/api/Auth/login', { email: this.email, password: this.password, rememberMe: this.rememberMe })
            .then(res => 
                {
                    this.$router.push('/dashboard');
                }
            )
            .catch(error => {
                    console.log("Unauthorized");              
                    console.error(error);
                    this.errorMessage = 'Please log in with valid credentials.';
            });
        }
    },
    computed: {
        
        logo() {
          return logoblue;
        },
        petplay() {
          return petplay;
        },
        pick() {
          return pick;
        },
        authThemeMask() {
          return authThemeMask;
        },
        
      },
}
</script>

<template>
    <!-- eslint-disable vue/no-v-html -->
  
    <div class="auth-wrapper d-flex align-center justify-center pa-4">
      <VCard
        class="auth-card pa-4 pt-7"
        max-width="448"
      >
        <VCardItem class="justify-center">
          
            <template #prepend>
          <div class="d-flex">
            <div v-html="Logo" />
          </div>
        </template>
          <VCardTitle class="font-weight-semibold text-2xl text-uppercase">
          </VCardTitle>
        </VCardItem>
  
        <VCardText class="pt-2">
          <h5 class="text-h5 font-weight-semibold mb-1">
            Welcome to UTask!
          </h5>
          <p class="mb-0">
            Please sign in to your account and begin your journey!
          </p>
        </VCardText>
  
        <VCardText>
          <VForm @submit.prevent="submitForm">
            <VRow>
              <!-- email -->
              <VCol cols="12">
                <VTextField
                  v-model="email"
                  label="Email"
                  type="email"
                />
              </VCol>
  
              <!-- password -->
              <VCol cols="12">
                <VTextField
                  v-model="password"
                  label="Password"
                  placeholder="············"
                  type="password"

                />
  
                <!-- remember me checkbox -->
                <div class="d-flex align-center justify-space-between flex-wrap mt-1 mb-4">
                  <VCheckbox
                    v-model="rememberMe"
                    label="Remember me"
                  />
  
                  <a
                    class="ms-2 mb-1"
                    href="javascript:void(0)"
                  >
                    Forgot Password?
                  </a>
                </div>
  
                <!-- login button -->
                <VBtn
                  block
                  type="submit"
                >
                  Login
                </VBtn>
              </VCol>
  
              <!-- create account -->
              <VCol
                cols="12"
                class="text-center text-base"
              >
                <span>New on our platform?</span>
                <RouterLink
                  class="text-primary ms-2"
                  to="/register"
                >
                  Create an account
                </RouterLink>
              </VCol>
  
              
              <!-- auth providers -->
              <VCol
                cols="12"
                class="text-center"
              >
              </VCol>

              
            </VRow>
          </VForm>
        </VCardText>
      </VCard>
    
    <VImg class="auth-footer-start-tree d-none d-md-block" :src="petplay" :width="350"  />

    <VImg :src="pick" class="auth-footer-end-tree d-none d-md-block" :width="350"/>

    <VImg class="auth-footer-mask d-none d-md-block" :src="authThemeMask" />
    </div>
  </template>
  
  <style lang="scss">
  @use "../assets/scss/pages/page-auth.scss";
  </style>
  


