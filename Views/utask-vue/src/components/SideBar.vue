<template>
    <v-card>
      <v-layout>
        <v-navigation-drawer> 
          <v-list density="compact" nav>
            <v-list-item >
                        Access Account
                    </v-list-item>
                    <v-list-item >
                        View Notifications
                    </v-list-item>
                    <v-list-item >
                        Explore Service Categories
                    </v-list-item>
                    <v-list-item >
                        Search for Services
                    </v-list-item>
                    <v-list-item >
                        Invoices and Orders
                    </v-list-item>
                    <v-list-item >
                        Manage Settings
                    </v-list-item>
                    <v-list-item>
                        Help and Support
                    </v-list-item>
                    <v-list-item>
                        Invite Friends
                    </v-list-item>
                    <v-list-item>
                        Logout
                    </v-list-item>
        </v-list>
        </v-navigation-drawer>

      </v-layout>
    </v-card>
  </template>
  
  <script>
export default {
    methods: {
      logoutUser() {
        this.$store.dispatch('logoutUser')
        this.$router.push('/')
      }
    }, 
};
</script>
  