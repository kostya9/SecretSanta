module.exports = {
    purge: {
        enabled: false,
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