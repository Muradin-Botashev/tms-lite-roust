import React, { useEffect, useState } from 'react';
import { sortFunc } from '../../utils/sort';
import {
    addToEnd,
    droppable,
    droppable2,
    entities as initial,
    getItems,
    multiDragAwareReorder,
    multiSelectTo as multiSelect
} from '../../utils/dragAndDropHelper';
import { DragDropContext } from 'react-beautiful-dnd';
import Column from './column';


const DragAndDrop = ({ left, right, t, search, onChange, type }) => {
    let [entities, setEntities] = useState({
        ...initial,
        columns: {
            [droppable.id]: {
                ...droppable,
                items: sortFunc(left.map(x => ({ id: x.displayNameKey, content: x })), t, 'id'),
            },
            [droppable2.id]: {
                ...droppable2,
                items: right.map(x => ({ id: x.displayNameKey, content: x })),
            },
        },
    });
    let [selected, setSelected] = useState([]);
    let [draggingId, setDraggingId] = useState(null);

    useEffect(
        () => {
            setEntities({
                ...entities,
                columns: {
                    [droppable.id]: {
                        ...droppable,
                        items: sortFunc(
                            left.map(x => ({ id: x.displayNameKey, content: x })),
                            t,
                            'id',
                        ),
                    },
                    [droppable2.id]: {
                        ...droppable2,
                        items: right.map(x => ({ id: x.displayNameKey, content: x })),
                    },
                },
            });
        },
        [left, right],
    );

    useEffect(() => {
        window.addEventListener('click', onWindowClick);
        window.addEventListener('keydown', onWindowKeyDown);
        window.addEventListener('touchend', onWindowTouchEnd);

        return () => {
            window.removeEventListener('click', onWindowClick);
            window.removeEventListener('keydown', onWindowKeyDown);
            window.removeEventListener('touchend', onWindowTouchEnd);
        };
    }, []);

    const onWindowClick = event => {
        if (event.defaultPrevented) {
            return;
        }
        unselectAll();
    };

    const onWindowKeyDown = event => {
        if (event.defaultPrevented) {
            return;
        }

        if (event.key === 'Escape') {
            unselectAll();
        }
    };

    const onWindowTouchEnd = event => {
        if (event.defaultPrevented) {
            return;
        }
        unselectAll();
    };

    const onDragStart = start => {
        const id = start.draggableId;
        const selectedItem = selected.find(item => item.id === id);

        // if dragging an item that is not selected - unselect all items
        if (!selectedItem) {
            unselectAll();
        }

        setDraggingId(start.draggableId);
    };

    const onDragEnd = result => {
        const { source, destination } = result;

        if (!destination || result.reason === 'CANCEL') {
            setDraggingId(null);
            return;
        }

        const processed = multiDragAwareReorder({
            entities,
            selected,
            source,
            destination,
        });

        setEntities(processed.entities);
        setSelected(processed.selected);
        setDraggingId(null);

        onChange(processed.entities.columns[droppable2.id].items.map(x => x.content));
    };

    const toggleSelection = item => {
        const wasSelected = selected.map(item => item.id).includes(item.id);

        const newSelected = (() => {
            // Task was not previously selected
            // now will be the only selected item
            if (!wasSelected) {
                return [item];
            }

            // Task was part of a selected group
            // will now become the only selected item
            if (selected.length > 1) {
                return [item];
            }

            // task was previously selected but not in a group
            // we will now clear the selection
            return [];
        })();

        setSelected(newSelected);
    };

    const toggleSelectionInGroup = item => {
        const index = selected.map(item => item.id).indexOf(item.id);

        // if not selected - add it to the selected items
        if (index === -1) {
            setSelected([...selected, item]);
            return;
        }

        // it was previously selected and now needs to be removed from the group
        const shallow = [...selected];
        shallow.splice(index, 1);
        setSelected(shallow);
    };

    const multiSelectTo = item => {
        const updated = multiSelect(entities, selected, item, search, t);

        if (updated == null) {
            return;
        }

        setSelected(updated);
    };

    const doubleClick = (item, columnId, index) => {
        const processed = addToEnd(item, entities, columnId, index);

        setEntities(processed.entities);

        onChange(processed.entities.columns[droppable2.id].items.map(x => x.content));
    };

    const unselectAll = () => {
        setSelected([]);
    };

    return (
        <DragDropContext onDragStart={onDragStart} onDragEnd={onDragEnd}>
            {entities.columnOrder.map(columnId => (
                <Column
                    column={entities.columns[columnId]}
                    items={getItems(entities, columnId)}
                    selected={selected}
                    key={columnId}
                    t={t}
                    search={search}
                    draggingId={draggingId}
                    toggleSelection={toggleSelection}
                    toggleSelectionInGroup={toggleSelectionInGroup}
                    multiSelectTo={multiSelectTo}
                    doubleClick={doubleClick}
                />
            ))}
        </DragDropContext>
    );
};

export default DragAndDrop;
