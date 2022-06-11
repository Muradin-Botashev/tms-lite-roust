import React, { useMemo } from 'react';
import { Droppable } from 'react-beautiful-dnd';
import Item from './item';

const Column = ({
    column,
    items,
    selected,
    draggingId,
    toggleSelection,
    toggleSelectionInGroup,
    multiSelectTo,
    doubleClick,
    t,
    search,
}) => {
    const selectedMap = useMemo(
        () => {
            return selected.map(item => item.id);
        },
        [selected],
    );

    return (
        <div style={{ width: '49%' }}>
            <p style={{ textAlign: 'center' }}>{t(column.title)}</p>
            <Droppable droppableId={column.id}>
                {(provided, snapshot) => (
                    <div
                        ref={provided.innerRef}
                        style={{
                            padding: 5,
                            width: '100%',
                            border: '1px solid #eee',
                            height: 'calc(55vh - 50px)',
                            minHeight: 200,
                            overflowY: 'auto',
                            overflowX: 'hidden',
                        }}
                    >
                        {items.map((item, index) => {
                            const isSelected = selectedMap.includes(item.id);
                            const isGhosting =
                                isSelected && Boolean(draggingId) && draggingId !== item.id;
                            return (
                                <Item
                                    item={item}
                                    index={index}
                                    key={item.id}
                                    isSelected={isSelected}
                                    isGhosting={isGhosting}
                                    isDragging={draggingId}
                                    t={t}
                                    droppableId={column.id}
                                    search={search}
                                    selectionCount={selected.length}
                                    toggleSelection={toggleSelection}
                                    toggleSelectionInGroup={toggleSelectionInGroup}
                                    multiSelectTo={multiSelectTo}
                                    doubleClick={doubleClick}
                                />
                            );
                        })}
                        {provided.placeholder}
                    </div>
                )}
            </Droppable>
        </div>
    );
};

export default Column;
