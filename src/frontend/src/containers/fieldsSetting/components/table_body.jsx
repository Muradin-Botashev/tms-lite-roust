import React, { useCallback, useState, useEffect } from 'react';
import { Table } from 'semantic-ui-react';
import FieldCell from './field_cell';
import SettingCell from './setting_cell';
import CellValue from '../../../components/ColumnsValue';
import { ACTIVE_TYPE } from '../../../constants/columnTypes';
import _ from 'lodash';

const TableBody = ({
    statusList,
    changeSettings,
    isExt,
    editProgress,
    t,
    toggleHidden,
    column
}) => {
    let [toggle, setToggle] = useState(column.isHidden);

    useEffect(
        () => {
            setToggle(column.isHidden);
        },
        [column.isHidden],
    );

    useEffect(
        () => {
            if (toggle !== column.isHidden) {
                toggleHidden(column.fieldName, isExt);
            }
        },
        [toggle],
    );

    const handleOnChange = useCallback(
        (e, { value, fieldName, status }) => {
            changeSettings(fieldName, value, status, isExt);
        },
        [isExt],
    );

    return (
        <Table.Row key={column.fieldName}>
            <Table.Cell className="table-fields-setting_name">
                <FieldCell
                    field={column.fieldName}
                    fieldName={column.displayName}
                    isDisabled={column.isReadOnly || toggle}
                    isExt={isExt}
                    t={t}
                    changeSettings={changeSettings}
                />
            </Table.Cell>
            <Table.Cell>
                <CellValue
                    value={toggle}
                    type={ACTIVE_TYPE}
                    toggleIsActive={() => setToggle(!toggle)}
                />
            </Table.Cell>
            {statusList.map(status => (
                <Table.Cell key={`${status.name}_${column.fieldName}`}>
                    <SettingCell
                        value={column.accessTypes && column.accessTypes[status.name]}
                        isDisabled={column.isReadOnly || toggle}
                        loading={
                            editProgress &&
                            (editProgress.field === column.fieldName &&
                                (!editProgress.state || editProgress.state === status.name))
                        }
                        status={status.name}
                        fieldName={column.fieldName}
                        onChange={handleOnChange}
                        t={t}
                    />
                </Table.Cell>
            ))}
        </Table.Row>
    );
};

export default React.memo(TableBody, (prevProps, currentProps) => {
    return _.isEqual(prevProps.column, currentProps.column)
});
