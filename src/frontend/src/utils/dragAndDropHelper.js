export const droppable = {
    id: 'droppable',
    title: 'Available',
};

export const droppable2 = {
    id: 'droppable2',
    title: 'Selected',
};

export const entities = {
    columnOrder: [droppable.id, droppable2.id],
    columns: {
        [droppable.id]: droppable,
        [droppable2.id]: droppable2,
    },
};

export const getItems = (entities, columnId) => entities.columns[columnId].items;

export const reorder = (list, startIndex, endIndex) => {
    const result = Array.from(list);
    const [removed] = result.splice(startIndex, 1);
    result.splice(endIndex, 0, removed);

    return result;
};

export const move = (source, destination, droppableSource, droppableDestination, search) => {
    const sourceClone = Array.from(source);
    const destClone = Array.from(destination);
    const [removed] = sourceClone.splice(droppableSource.index, 1);

    destClone.splice(search ? destClone.length : droppableDestination.index, 0, removed);

    const result = {};
    result[droppableSource.droppableId] = sourceClone;
    result[droppableDestination.droppableId] = destClone;

    return result;
};

export const getItemStyle = (isDragging, draggableStyle, isSelected, isGhosting) => {
    const modalEl = document.getElementById('fieldModal');
    if (modalEl && /Chrome/.test(navigator.userAgent)) {
        let rect = modalEl.getBoundingClientRect();
        draggableStyle.top = draggableStyle.top - rect.top || 0;
        draggableStyle.left = draggableStyle.left - rect.left || 0;
    }

    return {
        userSelect: 'none',
        padding: 5,
        ...draggableStyle,
    };
};

export const getLabelStyle = (isSelected, isGhosting, isDragging) => {
    let style = {
        width: '100%',
    };

    if (isSelected) {
        style = {
            ...style,
            background: '#18A8CC',
            color: 'rgba(255, 255, 255, 0.8)',
        };
    }

    if (isGhosting && isDragging) {
        style = {
            ...style,
            /*background: '#e3f5f9',
            color: '#4d4d4d',*/
            display: 'none'
        };
    }

    return style;
};

export const multiDragAwareReorder = args => {
    if (args.selected.length > 1) {
        return reorderMultiDrag(args);
    }
    return reorderSingleDrag(args);
};

export const addToEnd = (item, entities, columnId, index) => {
    const home = entities.columns[columnId];
    const foreign = entities.columns[columnId === droppable.id ? droppable2.id : droppable.id];

    // remove from home column
    const newHomeItems = [...home.items];
    newHomeItems.splice(index, 1);

    // add to foreign column
    const newForeignItems = [...foreign.items];
    newForeignItems.splice(newForeignItems.length, 0, item);

    const updated = {
        ...entities,
        columns: {
            ...entities.columns,
            [home.id]: withNewTaskIds(home, newHomeItems),
            [foreign.id]: withNewTaskIds(foreign, newForeignItems),
        },
    };

    return {
        entities: updated,
    };
};

const reorderMultiDrag = ({ entities, selected, source, destination }) => {
    const start = entities.columns[source.droppableId];
    const mapSelected = selected.map(item => item.id);

    const dragged = start.items[source.index];

    const insertAtIndex = (() => {
        const destinationIndexOffset = mapSelected.reduce((previous, current) => {
            if (current === dragged) {
                return previous;
            }

            const final = entities.columns[destination.droppableId];
            const column = getHomeColumn(entities, current);

            if (column !== final) {
                return previous;
            }

            const index = column.items.indexOf(current);

            if (index >= destination.index) {
                return previous;
            }

            // the selected item is before the destination index
            // we need to account for this when inserting into the new location
            return previous + 1;
        }, 0);

        const result = destination.index - destinationIndexOffset;
        return result;
    })();

    const withRemovedTasks = entities.columnOrder.reduce((previous, columnId) => {
        const column = entities.columns[columnId];

        // remove the id's of the items that are selected
        const remainingTaskIds = column.items.filter(item => !mapSelected.includes(item.id));

        previous[column.id] = withNewTaskIds(column, remainingTaskIds);
        return previous;
    }, entities.columns);

    const final = withRemovedTasks[destination.droppableId];
    const withInserted = (() => {
        const base = [...final.items];
        base.splice(insertAtIndex, 0, ...selected);
        return base;
    })();

    // insert all selected tasks into final column
    const withAddedTasks = {
        ...withRemovedTasks,
        [final.id]: withNewTaskIds(final, withInserted),
    };

    const updated = {
        ...entities,
        columns: withAddedTasks,
    };

    return {
        entities: updated,
        selected,
    };
};

const reorderSingleDrag = ({ entities, selected, source, destination }) => {
    if (source.droppableId === destination.droppableId) {
        const column = entities.columns[source.droppableId];
        const reordered = reorder(column.items, source.index, destination.index);

        const updated = {
            ...entities,
            columns: {
                ...entities.columns,
                [column.id]: withNewTaskIds(column, reordered),
            },
        };

        return {
            entities: updated,
            selected,
        };
    }

    const home = entities.columns[source.droppableId];
    const foreign = entities.columns[destination.droppableId];

    // the id of the task to be moved
    const taskId = home.items[source.index];

    // remove from home column
    const newHomeTaskIds = [...home.items];
    newHomeTaskIds.splice(source.index, 1);

    // add to foreign column
    const newForeignTaskIds = [...foreign.items];
    newForeignTaskIds.splice(destination.index, 0, taskId);

    const updated = {
        ...entities,
        columns: {
            ...entities.columns,
            [home.id]: withNewTaskIds(home, newHomeTaskIds),
            [foreign.id]: withNewTaskIds(foreign, newForeignTaskIds),
        },
    };

    return {
        entities: updated,
        selected,
    };
};

export const getHomeColumn = (entities, item) => {
    const columnId = entities.columnOrder.find(id => {
        const column = entities.columns[id];
        return column.items.map(map => map && map.id).includes(item.id);
    });

    return entities.columns[columnId];
};

const withNewTaskIds = (column, items) => ({
    id: column.id,
    title: column.title,
    items,
});

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
