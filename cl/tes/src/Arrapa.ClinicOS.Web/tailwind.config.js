/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        // Primary — trustworthy medical blue
        brand: {
          25: '#F2F7FF',
          50: '#ECF3FF',
          100: '#DDE9FF',
          200: '#C2D6FF',
          300: '#9CB9FF',
          400: '#7592FF',
          500: '#465FFF',
          600: '#3641F5',
          700: '#2A31D8',
          800: '#252DAE',
          900: '#232E88',
          950: '#141AB0',
        },
        // Secondary — calming teal for accents
        teal: {
          50: '#EEFBF9',
          100: '#D5F5F0',
          200: '#AEEAE2',
          300: '#7BD9CE',
          400: '#43C2B7',
          500: '#14A89D',
          600: '#0C877F',
          700: '#0E6B65',
          800: '#115552',
          900: '#124745',
        },
        // Neutral — slate for text and surfaces
        gray: {
          25: '#FCFCFD',
          50: '#F9FAFB',
          100: '#F2F4F7',
          200: '#E4E7EC',
          300: '#D0D5DD',
          400: '#98A2B3',
          500: '#667085',
          600: '#475467',
          700: '#344054',
          800: '#1D2939',
          900: '#101828',
          950: '#0C111D',
        },
        // Semantic
        success: {
          50: '#ECFDF3',
          100: '#DCFAE6',
          500: '#12B76A',
          600: '#039855',
          700: '#027A48',
        },
        warning: {
          50: '#FFFAEB',
          100: '#FEF0C7',
          500: '#F79009',
          600: '#DC6803',
          700: '#B54708',
        },
        danger: {
          50: '#FEF3F2',
          100: '#FEE4E2',
          500: '#F04438',
          600: '#D92D20',
          700: '#B42318',
        },
      },
      fontFamily: {
        outfit: ['Outfit', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        sans: ['Outfit', 'ui-sans-serif', 'system-ui', 'sans-serif'],
      },
      fontSize: {
        // Kept as [size, lineHeight] tuples so @apply works reliably.
        // Weight & tracking are applied in CSS layer or utility classes (see styles/index.css).
        'display-lg': ['32px', '40px'],
        'display':    ['28px', '36px'],
        'title-lg':   ['20px', '28px'],
        'title':      ['18px', '24px'],
        'body-lg':    ['16px', '24px'],
        'body':       ['14px', '20px'],
        'caption':    ['12px', '16px'],
        'micro':      ['11px', '14px'],
      },
      borderRadius: {
        DEFAULT: '8px',
      },
      boxShadow: {
        // Micro elevations (subtle, layered)
        'theme-xs':  '0 1px 2px 0 rgba(16, 24, 40, 0.05)',
        'theme-sm':  '0 1px 3px 0 rgba(16, 24, 40, 0.08), 0 1px 2px -1px rgba(16, 24, 40, 0.06)',
        'theme-md':  '0 4px 8px -2px rgba(16, 24, 40, 0.08), 0 2px 4px -2px rgba(16, 24, 40, 0.04)',
        'theme-lg':  '0 12px 24px -6px rgba(16, 24, 40, 0.10), 0 4px 8px -4px rgba(16, 24, 40, 0.05)',
        'theme-xl':  '0 20px 40px -12px rgba(16, 24, 40, 0.14)',
        'ring-brand': '0 0 0 4px rgba(70, 95, 255, 0.14)',
        'ring-danger': '0 0 0 4px rgba(240, 68, 56, 0.14)',
        'ring-success': '0 0 0 4px rgba(18, 183, 106, 0.14)',
      },
      transitionTimingFunction: {
        'out-quart': 'cubic-bezier(0.25, 1, 0.5, 1)',
        'in-out-quart': 'cubic-bezier(0.76, 0, 0.24, 1)',
      },
      transitionDuration: {
        150: '150ms',
        200: '200ms',
      },
      keyframes: {
        'fade-in': {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        'slide-up': {
          '0%': { opacity: '0', transform: 'translateY(6px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        'slide-in-right': {
          '0%': { transform: 'translateX(100%)' },
          '100%': { transform: 'translateX(0)' },
        },
        shimmer: {
          '100%': { transform: 'translateX(100%)' },
        },
      },
      animation: {
        'fade-in': 'fade-in 150ms ease-out',
        'slide-up': 'slide-up 200ms ease-out',
        'slide-in-right': 'slide-in-right 200ms cubic-bezier(0.25, 1, 0.5, 1)',
        shimmer: 'shimmer 1.5s infinite',
      },
    },
  },
  plugins: [],
};
