import React, { useRef, useEffect, useState } from 'react';
import CellValue from '../../ColumnsValue';
import TextCropping from '../../ColumnsValue/TextCropping';

const BodyCellComponent = ({
    column,
    children,
    value,
    valueText,
    valueTooltip,
    indexColumn,
    indexRow,
    toggleIsActive,
    t,
    id,
    width,
}) => {
    const cellRef = useRef(null);

    return (
        <td
            className={column.isFixedPosition ? 'no-scroll no-scroll-value' : ''}
            ref={cellRef}
            style={
                column.isFixedPosition
                    ? {
                          left: `${150 * indexColumn}px`,
                          maxWidth: '150px',
                          minWidth: '150px',
                      }
                    : width
                    ? { maxWidth: `${width}px` }
                    : {
                          maxWidth: '600px',
                      }
            }
        >
            <CellValue
                {...column}
                toggleIsActive={toggleIsActive}
                indexRow={indexRow}
                indexColumn={indexColumn}
                value={value}
                valueText={valueText}
                valueTooltip={valueTooltip}
                width={column.isFixedPosition ? 150 : width ? width : 600}
                id={id}
                t={t}
            />
            {/*<TextCropping width={width} indexColumn={indexColumn}>
                {value}
            </TextCropping>*/}
        </td>
    );
};

export default React.memo(BodyCellComponent);
