import React, { Component } from 'react';
import { withTranslation } from 'react-i18next';
import { withRouter } from 'react-router-dom';
import { Button, Checkbox, Loader, Table } from 'semantic-ui-react';
import BodyCell from './body_cell';
import { connect } from 'react-redux';
import { checkForEditingRequest } from '../../../ducks/gridColumnEdit';
import { invokeMassUpdateRequest } from '../../../ducks/gridActions';
import _ from 'lodash';
import CellValue from '../../ColumnsValue';
import {ORDERS_GRID, SHIPPINGS_GRID} from '../../../constants/grids';
import CustomCheckbox from "../../BaseComponents/CustomCheckbox";
import {LINK_TYPE} from "../../../constants/columnTypes";

class Result extends Component {
    handleCheck = (e, row) => {
        const { selectedRows = new Set(), setSelected, onlyOneCheck, rows } = this.props;
        let newSelectedRows;

        if (e.nativeEvent.shiftKey && selectedRows.size > 0 && !selectedRows.has(row.id)) {
            newSelectedRows = new Set(selectedRows);
            const lastSelectedId = [...selectedRows][selectedRows.size - 1];

            const previousIndex = rows.findIndex(r => r.id === lastSelectedId) || 0;
            const currentIndex = rows.findIndex(r => r.id === row.id) || 0;

            if (currentIndex > previousIndex) {
                for (let index = previousIndex; index <= currentIndex; index++) {

                    newSelectedRows.add(rows[index].id);
                }
            } else {
                for (let index = previousIndex; index >= currentIndex; index--) {

                    newSelectedRows.add(rows[index].id);
                }
            }

        } else if (onlyOneCheck) {
            newSelectedRows = new Set();
            if (!selectedRows.has(row.id)) {
                newSelectedRows.add(row.id);
            }
        } else {
            newSelectedRows = new Set(selectedRows);
            newSelectedRows[!selectedRows.has(row.id) ? 'add' : 'delete'](row.id);
        }

        setSelected(newSelectedRows);
    };

    render() {
        const {
            columns = [],
            rows = [],
            goToCard,
            actions,
            isShowActions,
            selectedRows,
            loadList,
            disabledCheck,
            name,
            progress,
            t,
            checkForEditing,
            invokeMassUpdate,
        } = this.props;

        return (
            <Table.Body>
                {rows &&
                    rows.map((row, indexRow) => (
                        <Table.Row
                            key={row.id}
                            className={`grid-row${
                                selectedRows.has(row.id) ? ' grid-row-selected' : ''
                            }${row.highlightForConfirmed || (row.backlights && row.backlights.length) ? ' grid-row-marker' : ''}`}
                            data-grid-id={row.id}
                        >
                            <Table.Cell
                                key={row.id + 'checkbox'}
                                className="small-column"
                                onClick={e => {
                                    e.stopPropagation();
                                }}
                            >
                                <div
                                    className={`${row.highlightForConfirmed || (row.backlights && row.backlights.length) ? 'grid-marker' : ''}`}/>
                                <CustomCheckbox
                                    checked={!!selectedRows.has(row.id)}
                                    disabled={disabledCheck(row)}
                                    onChange={e => {
                                        this.handleCheck(e, row);
                                    }}
                                />
                            </Table.Cell>
                            {columns &&
                                columns.map((column, indexColumn) => (
                                    <BodyCell
                                        key={`cell_${row.id}_${column.name}`}
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
                                        status={row.status}
                                        rowId={column.type === LINK_TYPE ? column.source === SHIPPINGS_GRID && column.source !== name ? row.shippingId : row.id : row.id}
                                        rowNumber={
                                            name === ORDERS_GRID
                                                ? row.orderNumber
                                                : row.shippingNumber
                                        }
                                        column={column}
                                        indexRow={indexRow}
                                        indexColumn={indexColumn}
                                        loadList={loadList}
                                        gridName={name}
                                        goToCard={goToCard}
                                        t={t}
                                        checkForEditing={checkForEditing}
                                        invokeMassUpdate={invokeMassUpdate}
                                    />
                                ))}
                            <Table.Cell />
                            {isShowActions ? (
                                <Table.HeaderCell
                                    className="actions-column"
                                    onClick={e => {
                                        e.stopPropagation();
                                    }}
                                >
                                    {actions &&
                                        actions(row).map(action => (
                                            <Button
                                                key={row.id + action.name}
                                                actionname={action.name}
                                                actionbuttonname={action.buttonName}
                                                rowid={row.id}
                                                disabled={action.disabled}
                                                className="grid-action-btn"
                                                loading={
                                                    action.loadingId &&
                                                    action.loadingId.includes(row.id)
                                                }
                                                onClick={e =>
                                                    action.action(e, {
                                                        action,
                                                        row,
                                                        loadList,
                                                    })
                                                }
                                                size="mini"
                                            >
                                                {action.buttonName}
                                            </Button>
                                        ))}
                                </Table.HeaderCell>
                            ) : null}
                        </Table.Row>
                    ))}
                <div className="table-bottom-loader">
                    <Loader active={progress && rows.length} />
                </div>
            </Table.Body>
        );
    }
}

const mapDispatchToProps = dispatch => {
    return {
        checkForEditing: params => {
            dispatch(checkForEditingRequest(params));
        },
        invokeMassUpdate: params => {
            dispatch(invokeMassUpdateRequest(params));
        },
    };
};

export default withTranslation()(
    connect(
        null,
        mapDispatchToProps,
    )(Result),
);
