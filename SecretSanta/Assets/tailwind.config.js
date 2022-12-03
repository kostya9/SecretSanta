module.exports = {
    purge: {
        enabled: process.env.NODE_ENV === 'production',
        content: [
            '../**/*.html',
            '../**/*.razor',
            '../**/*.cshtml'
        ],
    },
    variants: {
        backgroundColor: ({after}) => after(['disabled']),
        textColor: ({after}) => after(['disabled']),
    },
    plugins: [],
}