import React, { useEffect, useRef, useState, useCallback } from 'react';
import { Checkbox, Table } from 'semantic-ui-react';
import { Resizable } from 'react-resizable';
import FacetField from '../../FilterComponents';
import CustomCheckbox from "../../BaseComponents/CustomCheckbox";

const Filter = props => {
    const {
        isShowActions,
        indeterminate,
        all,
        checkAllDisabled,
        setSelectedAll,
        columns,
        resizeColumn,
        extWidth,
        gridName,
        filters
    } = props;

    const handleResize = useCallback((e, { size, index }) => {
        resizeColumn(size, index);
    }, []);

    return (
        <Table.Row className="sticky-header">
            <Table.HeaderCell className="small-column">
                <CustomCheckbox
                    indeterminate={indeterminate}
                    checked={all}
                    multi
                    disabled={checkAllDisabled}
                    onChange={setSelectedAll}
                />
            </Table.HeaderCell>
            {columns &&
                columns.map((x, i) => (
                    <Resizable
                        key={`resizable_${x.name}`}
                        width={x.width}
                        height={0}
                        axis="x"
                        onResize={(e, { size }) => handleResize(e, { size, index: i })}
                    >
                        <Table.HeaderCell
                            key={'th' + x.name}
                            style={{ width: `${x.width}px` }}
                            className={`column-facet column-${x.name &&
                                x.name.toLowerCase().replace(' ', '-')}-facet`}
                        >
                            <FacetField
                                key={'facet' + x.name}
                                {...x}
                                index={i}
                                gridName={gridName}
                                setSort={props.setSort}
                                type={x.filterType}
                                value={x.filter}
                                setFilter={props.setFilter}
                                filters={filters}
                                handleResize={handleResize}
                            />
                        </Table.HeaderCell>
                    </Resizable>
                ))}
            <Table.HeaderCell style={{ width: extWidth > 0 ? extWidth : 0 }} />
            {isShowActions ? <Table.HeaderCell className="actions-column" /> : null}
        </Table.Row>
    );
};

export default Filter;
