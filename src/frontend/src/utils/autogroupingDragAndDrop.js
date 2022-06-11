import React from 'react';
import {getHomeColumn} from "./dragAndDropHelper";

export const IsDraggingContext = React.createContext(false);

export const multiSelectTo = (entities, selected, item, search, t) => {
    // Nothing already selected
    if (!selected.length) {
        return [item];
    }

    const columnOfNew = getHomeColumn(entities, item);

    const indexOfNew = columnOfNew.items.map(item => item.id).indexOf(item.id);


    const lastSelected = selected[selected.length - 1];
    const columnOfLast = getHomeColumn(entities, lastSelected);
    const indexOfLast = columnOfLast.items.map(item => item.id).indexOf(lastSelected.id);

    // multi selecting to another column
    // select everything up to the index of the current item
    if (columnOfNew !== columnOfLast) {
        return columnOfNew.items.slice(0, indexOfNew + 1);
    }

    // multi selecting in the same column
    // need to select everything between the last index and the current index inclusive

    // nothing to do here
    if (indexOfNew === indexOfLast) {
        return null;
    }

    const isSelectingForwards = indexOfNew > indexOfLast;
    const start = isSelectingForwards ? indexOfLast : indexOfNew;
    const end = isSelectingForwards ? indexOfNew : indexOfLast;

    const inBetween = columnOfNew.items.slice(start, end + 1).filter(item => t(item.id).toLowerCase().includes(search.toLowerCase()));

    // everything inbetween needs to have it's selection toggled.
    // with the exception of the start and end values which will always be selected

    const toAdd = inBetween.filter(item => {
        // if already selected: then no need to select it again
        if (selected.map(item => item.id).includes(item.id)) {
            return false;
        }
        return true;
    });

    const sorted = isSelectingForwards ? toAdd : [...toAdd].reverse();
    const combined = [...selected, ...sorted];

    return combined;
};
