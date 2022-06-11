import React, { Component, useMemo } from 'react';
import { IsDraggingContext } from '../../utils/autogroupingDragAndDrop';
import ReactDOM from 'react-dom';
import TableCell from './TableCell';
import CellValue from '../ColumnsValue';
import { ORDERS_GRID } from '../../constants/grids';

const getItemStyle = (isDragging, draggableStyle, isDragOccurring, isSelected, isGhosting) => {
    let style = {
        ...draggableStyle,
    };
    if (isDragging || isSelected) {
        style = {
            ...style,
            background: '#e3f5f9',
        };
    }

    if (isGhosting && isDragOccurring) {
        style = {
            ...style,
            opacity: '0',
        };
    }

    return style;
};

const table = document.createElement('div');
table.classList.add('my-super-cool-table-portal');
Object.assign(table.style, {
    margin: '0',
    padding: '0',
    border: '0',
    height: '0',
    width: '0',
    position: 'relative',
});
/*const tbody = document.createElement('tbody');
table.appendChild(tbody);*/

if (!document.body) {
    throw new Error('document.body required for example');
}

document.body.appendChild(table);

const RowComponent = ({ columns, isDragging, snapshot, isSelected, row, indexRow, t }) => {
    return (
        <>
            {columns.map((column, indexColumn) => (
                <TableCell
                    isDragOccurring={isDragging}
                    isDragging={snapshot.isDragging}
                    cellId={column.name}
                    isSelected={isSelected}
                >
                    <CellValue
                        {...column}
                        indexRow={indexRow}
                        indexColumn={indexColumn}
                        value={
                            row[column.name] && typeof row[column.name] === 'object'
                                ? row[column.name].value
                                : row[column.name]
                        }
                        valueText={
                            row[column.name] && typeof row[column.name] === 'object'
                                ? row[column.name].name
                                : null
                        }
                        valueTooltip={
                            row[column.name] && typeof row[column.name] === 'object'
                                ? row[column.name].tooltip
                                : null
                        }
                        width={column.width}
                        gridName={ORDERS_GRID}
                        t={t}
                    />
                </TableCell>
            ))}
        </>
    );
};

const TableRow = ({
    snapshot,
    columns,
    provided,
    rowId,
    row,
    indexRow,
    t,
    id,
    toggleSelection,
    toggleSelectionInGroup,
    multiSelectTo,
    selectionCount,
    selectedDND,
    draggingId,
}) => {
    const onClick = event => {
        if (event.defaultPrevented) {
            return;
        }

        if (event.button !== 0) {
            return;
        }

        // marking the event as used
        event.preventDefault();

        performAction(event);
    };

    const onTouchEnd = event => {
        if (event.defaultPrevented) {
            return;
        }

        // marking the event as used
        // we would also need to add some extra logic to prevent the click
        // if this element was an anchor
        event.preventDefault();
        toggleSelectionInGroup(row);
    };

    const onKeyDown = (event, provided, snapshot) => {
        if (event.defaultPrevented) {
            return;
        }

        if (snapshot.isDragging) {
            return;
        }

        if (event.keyCode !== 13) {
            return;
        }

        // we are using the event for selection
        event.preventDefault();

        performAction(event);
    };

    // Determines if the platform specific toggle selection in group key was used
    const wasToggleInSelectionGroupKeyUsed = event => {
        const isUsingWindows = navigator.platform.indexOf('Win') >= 0;
        return isUsingWindows ? event.ctrlKey : event.metaKey;
    };

    // Determines if the multiSelect key was used
    const wasMultiSelectKeyUsed = event => event.shiftKey;

    const performAction = event => {
        if (wasToggleInSelectionGroupKeyUsed(event)) {
            toggleSelectionInGroup(row);
            return;
        }

        if (wasMultiSelectKeyUsed(event)) {
            multiSelectTo(row);
            return;
        }

        toggleSelection(row);
    };

    const selectedMap = useMemo(
        () => {
            return selectedDND.map(item => item.id);
        },
        [selectedDND],
    );

    const child = (
        <IsDraggingContext.Consumer>
            {isDragging => {
                const shouldShowSelection = snapshot.isDragging && selectionCount > 1;
                const isSelected = selectedMap.includes(row.id);
                const isGhosting = isSelected && Boolean(draggingId) && draggingId !== row.id;
                return (
                    <>
                        <tr
                            id={id}
                            ref={provided.innerRef}
                            {...provided.draggableProps}
                            {...provided.dragHandleProps}
                            style={getItemStyle(
                                snapshot.isDragging,
                                provided.draggableProps.style,
                                isDragging,
                                isSelected,
                                isGhosting,
                            )}
                            onClick={onClick}
                            onTouchEnd={onTouchEnd}
                            onKeyDown={event => onKeyDown(event, provided, snapshot)}
                        >
                            <RowComponent
                                columns={columns}
                                row={row}
                                indexRow={indexRow}
                                isDragging={isDragging}
                                isSelected={isSelected}
                                snapshot={snapshot}
                                t={t}
                            />
                        </tr>
                    </>
                );
            }}
        </IsDraggingContext.Consumer>
    );

    if (!snapshot.isDragging) {
        return child;
    }

    const body = (
        <>
            <IsDraggingContext.Consumer>
                {isDragging => {
                    const isSelected = selectedMap.includes(row.id);
                    const rows = selectedDND && selectedDND.length ? selectedDND : [row];
                    return (
                        <table
                            id={id}
                            cellSpacing="0"
                            ref={provided.innerRef}
                            {...provided.draggableProps}
                            {...provided.dragHandleProps}
                            style={{
                                ...provided.draggableProps.style,
                                border: '1px solid #e6e6e6',
                            }}
                        >
                            <thead>
                                <tr>
                                    {columns.map(column => (
                                        <TableCell
                                            isDragOccurring={isDragging}
                                            isDragging={snapshot.isDragging}
                                            cellId={column.name}
                                            isHeader
                                        >
                                            {t(column.displayNameKey)}
                                        </TableCell>
                                    ))}
                                </tr>
                            </thead>
                            <tbody>
                                {rows.map(row => (
                                    <tr
                                        key={row.id}
                                        className="grid-row"
                                        style={{ background: '#e3f5f9', position: 'relative' }}
                                    >
                                        <RowComponent
                                            columns={columns}
                                            row={row}
                                            indexRow={indexRow}
                                            isDragging={isDragging}
                                            isSelected={isSelected}
                                            snapshot={snapshot}
                                            t={t}
                                        />
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    );
                }}
            </IsDraggingContext.Consumer>
        </>
    );
    return ReactDOM.createPortal(body, table);
};

export default TableRow;
