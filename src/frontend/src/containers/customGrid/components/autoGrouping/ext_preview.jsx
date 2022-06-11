import React, {forwardRef, useEffect, useState} from 'react';
import { Dimmer, Loader, Table } from 'semantic-ui-react';
import { useDispatch, useSelector } from 'react-redux';
import {
    clearExtPreview,
    extPreviewOrdersSelector,
    extPreviewProgressSelector,
    getExtPreviewRequest,
    previewExtColumnsSelector,
} from '../../../../ducks/autogrouping';
import { useTranslation } from 'react-i18next';
import CellValue from '../../../../components/ColumnsValue';
import { AUTOGROUPING_ORDERS_GRID, ORDERS_GRID } from '../../../../constants/grids';
import FacetField from '../../../../components/FilterComponents';
import { Draggable, Droppable } from 'react-beautiful-dnd';
import DragAndDropTable from "../../../../components/DragAndDropTable";

const ExtPreview = forwardRef(({ parentId, runId, ...rest }, ref) => {
    const { t } = useTranslation();
    const dispatch = useDispatch();

    let [filter, setFilter] = useState({});
    let [sort, setSort] = useState({});

    const columns = useSelector(previewExtColumnsSelector);
    const rows = useSelector(state => extPreviewOrdersSelector(state, parentId));
    const progress = useSelector(extPreviewProgressSelector);



    const featchData = () => {
        dispatch(
            getExtPreviewRequest({
                parentId,
                params: {
                    filter: {
                        ...filter,
                    },
                    sort,
                },
            }),
        );
    };
    ref.current = {
        data: {
            ...(ref.current ? ref.current.data : {}),
            [parentId]: {
                featchData,
            }
        }
    };

    useEffect(() => {
        featchData();

        return () => {
            //dispatch(clearExtPreview(parentId));
            setFilter({});
            setSort({});
        };
    }, []);

    useEffect(
        () => {
            featchData();
        },
        [filter, sort],
    );

    const handleChangeFilter = (e, { name, value }) => {
        setFilter(filter => {
            let newFilter = { ...filter };

            if (value) {
                newFilter = {
                    ...filter,
                    [name]: value,
                };
            } else {
                delete newFilter[name];
            }

            return newFilter;
        });
    };

    return (
        <div>
            {progress && <Loader active inline="centered" />}
            <Table>
                <Table.Header>
                    <Table.Row>
                        {columns.map((column, index) => (
                            <Table.HeaderCell className="collapsed-table-header">
                                <FacetField
                                    key={'facet' + column.name}
                                    index={index}
                                    name={column.name}
                                    gridName={`${AUTOGROUPING_ORDERS_GRID}/${runId}/${parentId}`}
                                    displayNameKey={column.displayNameKey}
                                    sort={sort && sort.name === column.name ? sort.desc : null}
                                    setSort={setSort}
                                    width={column.width}
                                    type={column.type}
                                    value={filter[column.name]}
                                    filters={{ filter }}
                                    setFilter={handleChangeFilter}
                                    source={column.source}
                                />
                            </Table.HeaderCell>
                        ))}
                    </Table.Row>
                </Table.Header>
                <DragAndDropTable
                    columns={columns}
                    rows={rows}
                    parentId={parentId}
                    t={t}
                    {...rest}
                />
            </Table>
        </div>
    );
});

export default ExtPreview;
