import { definePreset } from '@primevue/themes'
import Aura from '@primevue/themes/aura'

/**
 * Preset del ecosystem. Cambiar aquí los colores primarios para re-skin global.
 * Si el monolito Óptica System usa un color distinto, ajustar semantic.primary.
 */
export const OpticaPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50:  '{blue.50}',
      100: '{blue.100}',
      200: '{blue.200}',
      300: '{blue.300}',
      400: '{blue.400}',
      500: '{blue.500}',
      600: '{blue.600}',
      700: '{blue.700}',
      800: '{blue.800}',
      900: '{blue.900}',
      950: '{blue.950}'
    }
  }
})

export const primevueConfig = {
  theme: {
    preset: OpticaPreset,
    options: {
      darkModeSelector: '.dark',
      cssLayer: {
        name: 'primevue',
        order: 'tailwind-base, primevue, tailwind-utilities'
      }
    }
  },
  ripple: true
}

