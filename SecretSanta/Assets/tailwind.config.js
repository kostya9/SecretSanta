module.exports = {
    content: [
        '../**/*.html',
        '../**/*.razor',
        '../**/*.cshtml'
    ],
    theme: {
        extend: {
            colors: {
                cream: {
                    DEFAULT: '#FDF6E3',
                    dark: '#F5ECD7',
                },
                'warm-white': '#FFFBF5',
                'soft-red': '#D64545',
                'deep-red': '#B83B3B',
                burgundy: '#8B2635',
                'forest-green': '#2D5A3D',
                sage: '#7A9E7E',
                gold: {
                    DEFAULT: '#D4A853',
                    light: '#E8C97A',
                },
                brown: {
                    DEFAULT: '#5C4033',
                    light: '#8B7355',
                },
            },
            fontFamily: {
                display: ['Caveat', 'cursive'],
                body: ['Nunito', 'sans-serif'],
            },
            boxShadow: {
                'warm': '0 4px 6px rgba(92, 64, 51, 0.1), 0 1px 3px rgba(92, 64, 51, 0.1)',
                'warm-md': '0 8px 15px rgba(92, 64, 51, 0.15), 0 3px 6px rgba(92, 64, 51, 0.1)',
                'warm-lg': '0 12px 24px rgba(92, 64, 51, 0.2)',
            },
            borderRadius: {
                'cozy': '1rem',
                'cozy-lg': '1.25rem',
            },
        },
    },
    variants: {
        backgroundColor: ({after}) => after(['disabled']),
        textColor: ({after}) => after(['disabled']),
    },
    plugins: [],
}
