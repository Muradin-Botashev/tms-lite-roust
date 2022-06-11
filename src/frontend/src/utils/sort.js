export const sortFunc = (item, t, key) => {
    let new_item = [...item];
    new_item.sort(function(a, b) {
        const nameA = t(key ? a[key] : a).toLowerCase();
        const nameB = t(key ? b[key] : b).toLowerCase();
        if (nameA < nameB)
            //сортируем строки по возрастанию
            return -1;
        if (nameA > nameB) return 1;
        return 0; // Никакой сортировки
    });

    return new_item;
};
