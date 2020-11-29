module.exports = {
    variants: {
        backgroundColor: ({after}) => after(['disabled']),
        textColor: ({after}) => after(['disabled']),
    },
    plugins: [],
}