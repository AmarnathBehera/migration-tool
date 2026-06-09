module.exports = {
  content: ['./src/**/*.{ts,tsx}', './entry.tsx'],
  corePlugins: {
    preflight: false,
  },
  theme: {
    extend: {
      colors: {
        body: '#3e3e3c',
        label: '#525252',
        muted: '#707070',
        surface: '#ffffff',
        error: '#cb2929',
        outline: '#525252',
        separator: '#b0b0b0',
        heading: '#1d1d1b',
      },
      fontFamily: {
        display: ['"GT Walsheim"', 'sans-serif'],
      },
    },
  },
  plugins: [],
};
