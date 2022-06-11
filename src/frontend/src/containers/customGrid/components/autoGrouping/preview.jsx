import React, {forwardRef, useEffect, useState, useRef} from 'react';
import { useTranslation } from 'react-i18next';
import { Accordion, Table } from 'semantic-ui-react';
import CellValue from '../../../../components/ColumnsValue';
import { AUTOGROUPING_GRID, ORDERS_GRID } from '../../../../constants/grids';
import FacetField from '../../../../components/FilterComponents';
import CustomCheckbox from '../../../../components/BaseComponents/CustomCheckbox';
import { Resizable } from 'react-resizable';
import ExtPreview from './ext_preview';
import { DragDropContext } from 'react-beautiful-dnd';
import { IsDraggingContext } from '../../../../utils/autogroupingDragAndDrop';
import { moveOrdersRequest } from '../../../../ducks/autogrouping';
import { useDispatch } from 'react-redux';
import { Scrollbars } from 'react-custom-scrollbars';
import PreviewHeader from './previewHeader';

const Preview = forwardRef(({
    selectedRows,
    setSelectedAll,
    handleCheck,
    columns: columnsArray,
    preview,
    filter,
    changeFilter,
    width,
    sort,
    setSort,
    runId,
    loadData,
    getContainerWidth,
    ...rest
}, ref) => {
    const { t } = useTranslation();
    const dispatch = useDispatch();

    let [columns, setColumns] = useState([...columnsArray]);
    let [widthFull, setWidth] = useState(0);
    let [extWidth, setExtWidth] = useState(0);
    let [activeRows, setActiveRows] = useState([]);

    useEffect(
        () => {
            const newColumns = columns.map(item => ({
                ...item,
                width: item.width || 150,
            }));
            setColumns(newColumns);

            setWidth(newColumns.reduce((sum, item) => sum + 150, 0) + 65);
        },
        [width],
    );

    useEffect(() => {
        let newColumns = [];
        const nameColumnsCurrent = columns.map(item => item.name);
        columnsArray.forEach(col => {
            if (!nameColumnsCurrent.includes(col.name)) {
                newColumns.push({
                    ...col,
                    width: 150
                })
            } else {
                newColumns.push(columns.find(item => item.name === col.name))
            }
        });
        setColumns(newColumns);
        setWidth(newColumns.reduce((sum, item) => sum + 150, 0) + 65);
    }, [columnsArray]);

    useEffect(
        () => {
            getContainerWidth();
        },
        [activeRows],
    );

    const handleResize = (e, { size, index }) => {
        setColumns(columns => {
            const nextColumns = [...columns];
            nextColumns[index] = {
                ...nextColumns[index],
                width: size.width,
            };
            return nextColumns;
        });

        let sum = 0;

        columns.forEach(item => {
            sum = sum + item.width;
        });

        setExtWidth(width - 50 - sum);
        setWidth(sum + 50);
        getContainerWidth();
    };

    const handleRowClick = (e, { rowId }) => {
        const newActiveRows = new Set(activeRows);
        newActiveRows[newActiveRows.has(rowId) ? 'delete' : 'add'](rowId);
        setActiveRows(Array.from(newActiveRows));
    };

    return (
        <Table className="grid-table" style={{ minWidth: widthFull }} unstackable fixed>
            <PreviewHeader
                columns={columns}
                filter={filter}
                sort={sort}
                setSort={setSort}
                changeFilter={changeFilter}
                selectedRows={selectedRows}
                setSelectedAll={setSelectedAll}
                preview={preview}
                extWidth={extWidth}
                handleResize={handleResize}
                runId={runId}
            />
            <Table.Body>
                {preview.map((row, indexRow) => (
                    <React.Fragment key={`${row.id}`}>
                        <Accordion.Title
                            id={row.id}
                            as={Table.Row}
                            className={`grid-row${
                                selectedRows.includes(row.id) ? ' grid-row-selected' : ''
                            } grid-row-autogrouping${
                                activeRows.includes(row.id) ? ' grid-row-active' : ''
                            }`}
                            active={activeRows.includes(row.id)}
                            rowId={row.id}
                            onClick={handleRowClick}
                        >
                            <Table.Cell
                                key={row.id + 'checkbox'}
                                style={{ paddingLeft: '12px' }}
                                className="small-column"
                                onClick={e => {
                                    e.stopPropagation();
                                }}
                            >
                                <CustomCheckbox
                                    checked={!!selectedRows.includes(row.id)}
                                    onChange={() => {
                                        handleCheck(row);
                                    }}
                                />
                            </Table.Cell>
                            {columns.map((column, indexColumn) => (
                                <Table.Cell
                                    key={`cell_${row.id}_${column.name}`}
                                    className={`value-cell color_cell_${row[column.name] &&
                                        row[column.name].color}`}
                                >
                                    <div className="cell-grid">
                                        <div
                                            className={`cell-grid-value${
                                                row[column.name] ? '' : ' cell-grid-value_empty'
                                            }`}
                                        >
                                            <CellValue
                                                {...column}
                                                indexRow={indexRow}
                                                indexColumn={indexColumn}
                                                rowId={row.id}
                                                value={
                                                    row[column.name] &&
                                                    typeof row[column.name] === 'object'
                                                        ? row[column.name].value
                                                        : row[column.name]
                                                }
                                                valueText={
                                                    row[column.name] &&
                                                    typeof row[column.name] === 'object'
                                                        ? row[column.name].name
                                                        : null
                                                }
                                                valueTooltip={
                                                    row[column.name] &&
                                                    typeof row[column.name] === 'object'
                                                        ? row[column.name].tooltip
                                                        : null
                                                }
                                                alternativeCosts={
                                                    row[column.name] &&
                                                    row[column.name].alternativeCosts
                                                }
                                                width={column.width}
                                                gridName={ORDERS_GRID}
                                                t={t}
                                                runId={runId}
                                                loadData={loadData}
                                            />
                                        </div>
                                    </div>
                                </Table.Cell>
                            ))}
                            <Table.Cell />
                        </Accordion.Title>
                        {activeRows.includes(row.id) ? (
                            <Accordion.Content active={true} as={Table.Row}>
                                <Table.Cell
                                    colSpan={columns.length + 2}
                                    style={{ padding: '32px' }}
                                >
                                    <div className="collapsed-table">
                                        <Scrollbars
                                            autoHide
                                            autoHeight
                                            autoHeightMin={100}
                                            autoHeightMax={250}
                                            renderTrackVertical={props => (
                                                <div {...props} className="track-vertical" />
                                            )}
                                        >
                                            <ExtPreview parentId={row.id} runId={runId} ref={ref} {...rest} />
                                        </Scrollbars>
                                    </div>
                                </Table.Cell>
                            </Accordion.Content>
                        ) : null}
                    </React.Fragment>
                ))}
            </Table.Body>
        </Table>
    );
});

export default Preview;
