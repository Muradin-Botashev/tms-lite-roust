import React from 'react';
import { Draggable, Droppable } from 'react-beautiful-dnd';
import TableRow from './TableRow';

const style = {
    display: 'flex',
    flexDirection: 'column',
    paddingBottom: 0,
    transition: 'background-color 0.2s ease, opacity 0.1s ease',
    userSelect: 'none',
    width: '250px',
};

const DragAndDropTable = ({ rows, parentId, columns, t, ...rest }) => {
    return (
        <Droppable droppableId={parentId} ignoreContainerClipping={false}>
            {(droppableProvided, snapshot) => (
                <tbody ref={droppableProvided.innerRef} {...droppableProvided.droppableProps}>
                    {rows.map((row, indexRow) => (
                        <Draggable key={row.id} draggableId={row.id} index={indexRow}>
                            {(provided, snapshot) => (
                                <TableRow
                                    id={row.id}
                                    provided={provided}
                                    snapshot={snapshot}
                                    columns={columns}
                                    rowId={row.id}
                                    row={row}
                                    indexRow={indexRow}
                                    t={t}
                                    {...rest}
                                />
                            )}
                        </Draggable>
                    ))}
                    {droppableProvided.placeholder}
                </tbody>
            )}
        </Droppable>
    );
};

export default DragAndDropTable;
