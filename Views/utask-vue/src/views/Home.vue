<template>
    <v-app>
        <v-app-bar app>
            <template v-slot:append>
            <v-btn to="/register"><v-icon>mdi mdi-account-plus-outline</v-icon></v-btn>|
            <v-btn to="/login"><v-icon>mdi mdi-login</v-icon></v-btn>
            <v-btn to="/profile"><v-icon>mdi mdi-account</v-icon></v-btn>
            </template>
        </v-app-bar>

        <v-container>
            <v-card>
                <v-card-title>Form Section</v-card-title>
                <v-card-text>
                    <v-form>
                        <v-select label="What Service do you need"></v-select>
                        <v-select label="Where"></v-select>
                        <v-select label="When"></v-select>
                        <v-btn>Find a Utasker</v-btn>
                    </v-form>
                </v-card-text>
            </v-card>
            <v-spacer></v-spacer>
            <v-card>
                <v-card-title>Tutorial Video Section</v-card-title>
                <v-card-text>
                    <video width="400" controls>
                    <source src="mov_bbb.mp4" type="video/mp4">
                    </video>
                </v-card-text>
            </v-card>
            <v-spacer></v-spacer>
            <v-card>
                <v-card-title>Service Categories Display</v-card-title>
                <v-card-text>
                    <v-row>
                    <v-col v-for="(service, index) in services" :key="index" cols="3">
                    <v-card outlined>
                        <v-img :src="service.image" height="200"></v-img>
                        <v-card-title>{{ service.name }}</v-card-title>
                        <v-card-subtitle>{{ service.category }}</v-card-subtitle>
                        <v-card-text>{{ service.description }}</v-card-text>
                    </v-card>
                    </v-col>
                    </v-row>
                </v-card-text>

            </v-card>
            <v-spacer></v-spacer>
            <v-card>
                <v-card-title>Reviews</v-card-title>
                <v-row>
      <v-col v-for="(person, index) in persons" :key="index" cols="4">
        <v-card class="mx-auto" elevation="3" width="350">
          <div class="d-flex justify-between">
            <v-img
              contain
              height="100px"
              :src="person.image"
              style="flex-basis: 100px"
              class="flex-grow-0"
            ></v-img>
            <v-card-title>
              <div class="text-h6 font-weight-thin">{{ person.name }}</div>
            </v-card-title>
            <v-rating
              v-model="person.rating"
              color="white"
              active-color="yellow-accent-4"
              half-increments
              hover
              size="18"
            ></v-rating>
          </div>
          <v-card-text>{{ person.description }}</v-card-text>
        </v-card>
      </v-col>
    </v-row>
            </v-card>
            <v-spacer></v-spacer>

            <v-card>
                <v-card-title>Affiliated Media Icons</v-card-title>
                <v-card-text>
                    <v-row>
                        <v-col>
                            <v-icon color="red">mdi-television</v-icon> <!-- CNN -->
                        </v-col>
                        <v-col>
                            <v-icon color="black">mdi-television-classic</v-icon> <!-- BBC -->
                        </v-col>
                        <v-col>
                            <v-icon color="blue">mdi-television-guide</v-icon> <!-- CBC -->
                        </v-col>
                        <v-col>
                            <v-icon color="green">mdi mdi-firefox</v-icon> <!-- Al Jazeera -->
                        </v-col>
                        <v-col>
                            <v-icon color="orange">mdi-television-box</v-icon> <!-- NHK -->
                        </v-col>
                        <v-col>
                            <v-icon color="yellow">mdi-television-off</v-icon> <!-- ABC -->
                        </v-col>
                    </v-row>
                </v-card-text>
            </v-card>

            <v-card>
                <v-card-title>Invite Friends Call to Action</v-card-title>
                <v-card-text>
                    <!-- On click call an api -->
                    <v-btn 
                        color="primary"
                        dark
                        @click="callApi()"
                    >Invite Friends</v-btn>
                </v-card-text>
            </v-card>

            <v-footer>
                <span>&copy; 2024</span>
            </v-footer>
        </v-container>
    </v-app>
</template>

<script>
export default {
    name: 'Home',
    data() {
        return {
            services: [
            {
                name: 'Snow Removal',
                category: 'Outdoor',
                description: 'Clearing snow from driveways, walkways, and outdoor areas to ensure safe access during winter weather conditions.',
                image: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ8ArdMN6uau7slFtohtz90mbfQ9-YpI-0D2A&usqp=CAU'
            },
            {
                name: 'Garbage Collection',
                category: 'Specialized',
                description: 'Responsible collection and disposal of waste and recyclables from residential or commercial areas.',
                image: 'https://capitalcurrent.ca/wp-content/uploads/2021/08/Recycling-Bin-sm.jpg'
            },
            {
                name: 'Yard Maintenance',
                category: 'Outdoor',
                description: 'Providing upkeep for outdoor spaces including mowing lawns, basic landscaping tasks, and garden maintenance.',
                image: 'https://www.angelslandscaping.com/wp-content/uploads/2021/10/yad-and-gardden-cleanup.jpg'
            },
            {
                name: 'General Cleaning',
                category: 'Indoor',
                description: 'Thorough cleaning of residential or commercial spaces, ensuring cleanliness and organization.',
                image: 'https://images.squarespace-cdn.com/content/v1/57ade0b9e58c6256b36898d4/bdfc5deb-9c4d-4acc-9422-d81e24038126/2.jpg'
            },
            {
                name: 'Errand Services',
                category: 'Indoor',
                description: 'Assisting with everyday tasks like grocery shopping or parcel delivery, providing convenience to clients.',
                image: 'https://media.istockphoto.com/id/1426288045/video/close-up-of-unrecognizable-man-walking-down-the-street-carrying-a-tote-bag-with-groceries.jpg?s=640x640&k=20&c=tvGHN7JRmZInwYYNQbUcmeqaON-F0FASEQyGn-4hoU4='
            },
            {
                name: 'Pet Sitting/Walking',
                category: 'Indoor',
                description: 'Caring for pets while owners are away, including feeding, walking, and providing companionship.',
                image: 'https://yorkprofessionalpetsitting.com/wp-content/uploads/2023/06/How-Dog-Walking-Keeps-Your-Pet-Healthy.jpg'
            },
            {
                name: 'Event Planning/Support',
                category: 'Specialized',
                description: 'Organizing and managing various aspects of events, offering support in logistics, coordination, and execution to ensure successful gatherings or functions.',
                image: 'https://www.cvent.com/sites/default/files/styles/focus_scale_and_crop_800x450/public/image/2019-10/iStock-855485092.png?h=08b866d1&itok=twyjtv4R'
            },
            {
                name: 'Assembly and Installation',
                category: 'Indoor',
                description: 'Proficiently assembling furniture and installing fixtures or appliances, providing hassle-free setup and functionality for clients\' belongings.',
                image: 'https://blog.puls.com/hs-fs/hubfs/puls-furnture-assembly-services-included.jpg?width=2400&name=puls-furnture-assembly-services-included.jpg'
            }
            ],
            persons: [
        {
          name: 'Ellie Goulding',
          image: 'https://upload.wikimedia.org/wikipedia/commons/1/14/9-94702_user-outline-icon-clipart-png-download-profile-icon.png',
          rating: 4.5,
          description: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean scelerisque ligula est, consequat egestas dui bibendum sit amet. Suspendisse viverra, neque ut semper blandit, nisl ipsum consequat velit, pharetra varius tellus ante vel ipsum. Pellentesque tincidunt consequat ultricies.'
        },
        {
          name: 'John Doe',
          image: 'https://upload.wikimedia.org/wikipedia/commons/1/14/9-94702_user-outline-icon-clipart-png-download-profile-icon.png',
          rating: 3.8,
          description: 'Another person description lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.'
        },
        {
          name: 'Jane Smith',
          image: 'https://upload.wikimedia.org/wikipedia/commons/1/14/9-94702_user-outline-icon-clipart-png-download-profile-icon.png',
          rating: 4.0,
          description: 'A different person description lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.'
        },
      ]
        };
    },
    methods: {
        callApi() {
            console.log('Calling API');
            // fetch('http://localhost:5204/api/Auth/test',
            // {
            //     method: 'GET',
            //     headers: {
            //     'Content-Type': 'application/json',
            //     'Access-Control-Allow-Origin': 'http://localhost:8080/',
            //     },
            // }
            // )
            fetch('http://localhost:5204/api/Auth/test', {
            method: 'GET',
            headers: {
              'Content-Type': 'application/json',
              'Access-Control-Allow-Origin': 'http://localhost:8080/',
            },
          }) 
                .then(response => response.json())
                .then(data => console.log(data));

        }
    }
};
</script>
