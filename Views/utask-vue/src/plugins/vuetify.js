
import '@mdi/font/css/materialdesignicons.css'
import 'vuetify/styles'
import 'vuetify/dist/vuetify.min.css'
import { themes } from './themes/themes'
import { createVuetify } from 'vuetify'
const vuetify = createVuetify({
  theme: {
    defaultTheme: 'light',
    themes,
  },
})
export default vuetify
