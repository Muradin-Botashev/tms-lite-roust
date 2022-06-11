import React, { useRef, useEffect, useState, forwardRef } from 'react';
import FacetField from '../../FilterComponents';

const HeaderCellComponent = forwardRef(
    ({ row, index, setFilter, width, setSort, filters = {}, sort = {}, gridName }, ref) => {
        /* const cellRef = useRef(null);
    let [position, setPosition] = useState(null);

    useEffect(() => {
        setPosition(cellRef.current.offsetLeft);
    }, []);
*/
        return (
            <th
                className={
                    row.isFixedPosition ? 'no-scroll table-header-cell' : 'table-header-cell'
                }
                ref={ref}
                style={
                    row.isFixedPosition
                        ? {
                              left: index * 150,
                              maxWidth: '150px',
                              minWidth: '150px',
                          }
                        : width
                        ? {
                              maxWidth: `${width}px`,
                              minWidth: `${width}px`,
                          }
                        : { maxWidth: '600px' }
                }
            >
                <FacetField
                    key={'facet' + row.name}
                    notWrapHeader
                    index={index}
                    name={row.name}
                    gridName={gridName}
                    displayNameKey={row.displayNameKey}
                    sort={sort && sort.name === row.name ? sort.desc : null}
                    setSort={setSort}
                    type={row.type}
                    value={
                        filters.filter && filters.filter.filter && filters.filter.filter[row.name]
                    }
                    filters={filters}
                    setFilter={setFilter}
                    source={row.source}
                    noSort={row.isSortDisabled}
                    notSortAndFilter={row.notSortAndFilter}
                />
            </th>
        );
    },
);

export default React.memo(HeaderCellComponent);
