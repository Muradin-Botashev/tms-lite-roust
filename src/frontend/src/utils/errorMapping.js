export const errorMapping = array => {
    let error = {};
    if (array && array.length) {
        array.forEach(item => {
            error = {
                ...error,
                [item.name]: item.message,
            };
        });
    }
    return error;
};
