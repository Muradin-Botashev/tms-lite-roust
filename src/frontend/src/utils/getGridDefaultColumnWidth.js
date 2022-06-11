export default (width, count) => {
    return parseInt(width / count) > 100
        ? parseInt(width / count)
        : 100
}
