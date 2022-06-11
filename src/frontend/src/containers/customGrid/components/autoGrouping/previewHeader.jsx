import React, {useState, useEffect, useRef} from 'react';
import { Table } from 'semantic-ui-react';
import CustomCheckbox from '../../../../components/BaseComponents/CustomCheckbox';
import { Resizable } from 'react-resizable';
import FacetField from '../../../../components/FilterComponents';
import { AUTOGROUPING_GRID } from '../../../../constants/grids';
import ReactDOM from 'react-dom';

const table = document.createElement('div');
table.classList.add('table-header-portal');
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

const style = {
    zIndex: 25,
};

const PreviewHeader = ({
    columns,
    selectedRows,
    setSelectedAll,
    filter,
    sort,
    setSort,
    changeFilter,
    extWidth,
    preview,
    handleResize,
    runId,
}) => {
    const headerRef = useRef(null);

    const onScroll = () => {
        document.getElementById('headerRef').style.zIndex = '0';
        document.getElementById('headerRef').style.opacity = '0';
        setTimeout(() => {
            document.getElementById('fix-column').style.top = `${window.scrollY}px`;
            Array.from(document.getElementsByClassName('column-facet')).forEach(element => element.style.top = `${window.scrollY}px`);
            document.getElementById('headerRef').style.zIndex = '25';
            document.getElementById('headerRef').style.opacity = '1';
        }, 500);

    };

    useEffect(() => {
        window.addEventListener('scroll', onScroll);

        return () => {
            window.removeEventListener('scroll', onScroll);
        }
    }, []);

    return (
        <thead id="headerRef" style={style}>
            <Table.Row className="sticky-header">
                <Table.HeaderCell id="fix-column" className="small-column" style={{ paddingLeft: '12px' }}>
                    <CustomCheckbox
                        indeterminate={
                            !!(selectedRows.length && selectedRows.length !== preview.length)
                        }
                        checked={!!(selectedRows.length && selectedRows.length === preview.length)}
                        multi
                        onChange={setSelectedAll}
                    />
                </Table.HeaderCell>
                {columns.map((column, index) => (
                    <Resizable
                        key={`resizable_${column.name}`}
                        width={column.width}
                        height={0}
                        axis="x"
                        onResize={(e, { size }) => handleResize(e, { size, index })}
                    >
                        <Table.HeaderCell
                            key={column.name}
                            style={{ width: `${column.width}px` }}
                            className={`column-facet column-${column.name &&
                                column.name.toLowerCase().replace(' ', '-')}-facet`}
                        >
                            <FacetField
                                key={'facet' + column.name}
                                index={index}
                                name={column.name}
                                gridName={`${AUTOGROUPING_GRID}/${runId}`}
                                displayNameKey={column.displayNameKey}
                                sort={sort && sort.name === column.name ? sort.desc : null}
                                setSort={setSort}
                                width={column.width}
                                type={column.type}
                                value={filter[column.name]}
                                filters={{ filter }}
                                setFilter={changeFilter}
                                source={column.source}
                            />
                        </Table.HeaderCell>
                    </Resizable>
                ))}
                <Table.HeaderCell style={{ width: extWidth > 0 ? extWidth : 0 }} />
            </Table.Row>
        </thead>
    );
};

export default PreviewHeader;
