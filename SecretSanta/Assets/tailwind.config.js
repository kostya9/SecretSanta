module.exports = {
    purge: {
        enabled: true,
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
