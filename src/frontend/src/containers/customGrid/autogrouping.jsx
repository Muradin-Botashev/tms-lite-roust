import React, { useEffect, useRef, useState } from 'react';
import ReactDOM from 'react-dom';
import { useTranslation } from 'react-i18next';
import { useDispatch, useSelector } from 'react-redux';
import { Button, Form, Grid, Icon, Loader, Popup, Table } from 'semantic-ui-react';
import { Scrollbars } from 'react-custom-scrollbars';
import {
    applyAndSendAutoGroupingRequest,
    applyAutoGroupingRequest,
    applyProgressSelector,
    changeIsGrouping,
    exportProgressSelector,
    getPreviewRequest,
    infoSelector,
    isGroupingSelector,
    moveOrdersRequest,
    previewAutoGroupingRequest,
    previewColumnsSelector,
    previewExportToExcelRequest,
    previewOrdersSelector,
    previewProgressSelector,
    runIdSelector,
    settingsSelector,
} from '../../ducks/autogrouping';
import Preview from './components/autoGrouping/preview';
import CustomIcon from '../../components/CustomIcon';
import CheckBox from '../../components/BaseComponents/Checkbox';
import { DragDropContext } from 'react-beautiful-dnd';
import { IsDraggingContext, multiSelectTo as multiSelect } from '../../utils/autogroupingDragAndDrop';
import PreviewHeader from "./components/autoGrouping/previewHeader";

const AutoGrouping = props => {
    const { t } = useTranslation();
    const dispatch = useDispatch();

    const { location, history } = props;
    const { state } = location;
    const { pathname, selectedIds } = state;

    let [selectedRows, setSelectedRows] = useState([]);
    let [filters, setFilters] = useState({});
    let [sort, setSort] = useState({});
    let [settings, setSettings] = useState(null);
    let [isDragging, setIsDragging] = useState(false);
    let [containerWidth, setContainerWidth] = useState(0);
    let [selectedDND, setSelectedDND] = useState([]);
    let [draggingId, setDraggingId] = useState(null);
    let [col, setCol] = useState([]);

    const title = 'Формирование перевозок';
    const columns = useSelector(previewColumnsSelector);
    const preview = useSelector(state => previewOrdersSelector(state, filters, columns));
    const isGrouping = useSelector(isGroupingSelector);
    const previewProgress = useSelector(previewProgressSelector);
    const exportProgress = useSelector(exportProgressSelector);
    const applyProgress = useSelector(applyProgressSelector);
    const runId = useSelector(runIdSelector);
    const info = useSelector(infoSelector);
    const autogroupingTypes = useSelector(settingsSelector);

    const containerRef = useRef(null);
    const containerSRef = useRef(null);
    const containerСRef = useRef(null);

    const getContainerWidth = () => {
        setContainerWidth(
            containerRef && containerRef.current ? containerRef.current.offsetWidth : 0,
        );
        containerSRef && containerSRef.current && containerSRef.current.handleWindowResize();
    };

    useEffect(
        () => {
            getContainerWidth();
            window.addEventListener('resize', getContainerWidth);

            return () => window.removeEventListener('resize', getContainerWidth);
        },
        [containerRef.current],
    );

    useEffect(() => {
        document.body.style.overflowY = 'auto';
        getRunId(undefined, location.state && location.state.runId);
    }, []);

    useEffect(() => {
        window.addEventListener('click', onWindowClick);
        window.addEventListener('keydown', onWindowKeyDown);
        window.addEventListener('touchend', onWindowTouchEnd);

        return () => {
            window.removeEventListener('click', onWindowClick);
            window.removeEventListener('keydown', onWindowKeyDown);
            window.removeEventListener('touchend', onWindowTouchEnd);
        };
    }, []);

    useEffect(
        () => {
            // сохраняем runId в параметрах url, чтоб при перезагрузке страницы сохранялись изменения ТК
            history.replace({
                pathname: location.pathname,
                state: {
                    ...location.state,
                    runId,
                },
            });
        },
        [runId],
    );

    useEffect(
        () => {
            runId && getPreview();
        },
        [runId, filters, sort],
    );

    useEffect(
        () => {
            setSettings(autogroupingTypes);
        },
        [autogroupingTypes],
    );

    useEffect(
        () => {
            getContainerWidth();
            const idsArray = preview.map(item => item.id);

            setSelectedRows(selectedRows.filter(item => idsArray.includes(item)));
        },
        [preview],
    );

    const getRunId = (autogroupingTypes, runId) => {
        dispatch(
            previewAutoGroupingRequest({
                params: {
                    ids: selectedIds,
                    autogroupingTypes,
                },
                runId,
            }),
        );
    };

    const getPreview = () => {
        dispatch(
            getPreviewRequest({
                runId,
                params: {
                    filter: {
                        ...filters,
                    },
                    sort,
                },
            }),
        );
    };

    const handleChangeFilter = (e, { name, value }) => {
        setFilters(filter => {
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

    const clearFilter = () => {
        setFilters({});
    };

    const handleApply = () => {
        dispatch(
            applyAutoGroupingRequest({
                params: {
                    rowIds: selectedRows,
                },
                runId,
                callbackSuccess: () => {
                    handleClose();
                },
            }),
        );
    };

    const handleApplyAndSend = () => {
        dispatch(
            applyAndSendAutoGroupingRequest({
                params: {
                    rowIds: selectedRows,
                },
                runId,
                callbackSuccess: () => {
                    handleClose();
                },
            }),
        );
    };

    const exportExcel = () => {
        dispatch(
            previewExportToExcelRequest({
                runId,
                params: {
                    filter: {
                        ...filters,
                    },
                    sort,
                },
            }),
        );
    };

    const setSelectedAll = () => {
        if (selectedRows.length) {
            setSelectedRows([]);
        } else {
            setSelectedRows(preview.map(item => item.id));
        }
    };

    const handleCheck = row => {
        let newSelectedRows = new Set(selectedRows);

        newSelectedRows[!newSelectedRows.has(row.id) ? 'add' : 'delete'](row.id);
        setSelectedRows(Array.from(newSelectedRows));
    };

    const handleClose = () => {
        dispatch(changeIsGrouping(false));
        history.push({
            pathname,
        });
    };

    const handleChangeSettings = (e, { value, name }) => {
        setSettings(settings => ({
            ...settings,
            selected: value
                ? [...settings.selected, name]
                : settings.selected.filter(item => item.value !== name.value),
        }));
    };

    const onBeforeDragStart = e => {


        if (window.navigator.vibrate) {
            window.navigator.vibrate(100);
        }
    };

    const onDragStart = start => {
        const id = start.draggableId;
        const selectedItem = selectedDND.find(item => item.id === id);

        // if dragging an item that is not selected - unselect all items
        if (!selectedItem) {
            unselectAll();
        }

        setDraggingId(start.draggableId);
    };

    const onDragEnd = result => {
        const { draggableId, source, destination } = result;
        const { droppableId: sourceId = null } = source || {};
        const { droppableId: destinationId = null } = destination || {};

        setIsDragging(false);
        console.log('result', result, selectedDND);
        // dropped outside the list
        if (!result.destination) {
            return;
        }

        // no movement
        if (destinationId === sourceId) {
            return;
        }

        dispatch(
            moveOrdersRequest({
                runId,
                oldShipping: result.source.droppableId,
                params: {
                    newShippingId: result.destination.droppableId,
                    orderIds: selectedDND && selectedDND.length ? selectedDND.map(item => item.id) : [result.draggableId],
                },
                callbackSuccess: () => {
                    getPreview();
                    containerСRef.current.data[result.source.droppableId].featchData();
                    containerСRef.current.data[result.destination.droppableId].featchData();
                },
            }),
        );
    };

    const handleOnBeforeCapture = () => {
        console.log('capture');
        setIsDragging(true);
    };

    const onWindowClick = event => {
        if (event.defaultPrevented) {
            return;
        }
        unselectAll();
    };

    const onWindowKeyDown = event => {
        if (event.defaultPrevented) {
            return;
        }

        if (event.key === 'Escape') {
            unselectAll();
        }
    };

    const onWindowTouchEnd = event => {
        if (event.defaultPrevented) {
            return;
        }
        unselectAll();
    };

    const unselectAll = () => {
        setSelectedDND([]);
    };

    const toggleSelection = item => {
        console.log('toggleSelection');
        const wasSelected = selectedDND.map(item => item.id).includes(item.id);

        const newSelected = (() => {
            // Task was not previously selected
            // now will be the only selected item
            if (!wasSelected) {
                return [item];
            }

            // Task was part of a selected group
            // will now become the only selected item
            if (selectedDND.length > 1) {
                return [item];
            }

            // task was previously selected but not in a group
            // we will now clear the selection
            return [];
        })();

        setSelectedDND(newSelected);
    };

    const toggleSelectionInGroup = item => {
        console.log('toggleSelectionInGroup')
        const index = selectedDND.map(item => item.id).indexOf(item.id);

        // if not selected - add it to the selected items
        if (index === -1) {
            setSelectedDND([...selectedDND, item]);
            return;
        }

        // it was previously selected and now needs to be removed from the group
        const shallow = [...selectedDND];
        shallow.splice(index, 1);
        setSelectedDND(shallow);
    };

    const multiSelectTo = item => {
        console.log('multiSelectTo', item);
        //const updated = multiSelect({}, selectedDND, item);

       /* if (updated == null) {
            return;
        }

        setSelectedDND(updated);*/
    };

    console.log('selectedDND', selectedDND);

    return (
        <div className="container">
            {columns.length ? (
                <div>
                    <Loader active={previewProgress} size="huge" className="table-loader">
                        Loading
                    </Loader>
                    <Grid className="grid-header-panel grid-header-panel__noscroll">
                        <Grid.Row>
                            <Grid.Column width={5} verticalAlign="middle">
                                <span className="table-header-menu_title">
                                    <Button icon onClick={handleClose}>
                                        <Icon name="arrow left" />
                                    </Button>
                                    {t(title)}
                                </span>
                                <span className="records-counter">
                                    {t('totalCount', { count: preview.length })}
                                </span>
                            </Grid.Column>
                            <Grid.Column width={11} textAlign="right">
                                <Popup
                                    content={
                                        <Form>
                                            {settings && settings.all
                                                ? settings.all.map(item => (
                                                      <Form.Field key={item.value}>
                                                          <CheckBox
                                                              label={item.name}
                                                              name={item}
                                                              checked={settings.selected
                                                                  .map(s => s.value)
                                                                  .includes(item.value)}
                                                              onChange={handleChangeSettings}
                                                          />
                                                      </Form.Field>
                                                  ))
                                                : null}
                                            <Button
                                                className="autogrouping_link_button"
                                                disabled={!settings || !settings.selected || !settings.selected.length}
                                                onClick={() => getRunId(settings.selected || [])}
                                            >
                                                Предварительный расчет
                                            </Button>
                                        </Form>
                                    }
                                    position="bottom center"
                                    on="click"
                                    trigger={
                                        <div className="autogrouping_link">Параметры запуска</div>
                                    }
                                />
                                <Popup
                                    content={t('exportToExcel')}
                                    position="bottom right"
                                    trigger={
                                        <Button
                                            icon="download"
                                            loading={exportProgress}
                                            onClick={exportExcel}
                                        />
                                    }
                                />
                                <Popup
                                    content={t('reset_filters')}
                                    position="bottom right"
                                    trigger={
                                        <Button
                                            icon
                                            className={`clear-filter-btn`}
                                            onClick={clearFilter}
                                            disabled={!Object.keys(filters).length}
                                        >
                                            <CustomIcon name="clear-filter" />
                                        </Button>
                                    }
                                />
                            </Grid.Column>
                        </Grid.Row>
                    </Grid>
                    <div className="noscroll" ref={containerRef}>
                        <Scrollbars
                            ref={containerSRef}
                            autoHeight autoHeightMax={Number.MAX_VALUE}
                            renderView={props => <div {...props} className="view"/>}
                            renderTrackHorizontal={props => <div {...props} style={{width: containerWidth}} className="track-horizontal"/>}
                        >
                                <IsDraggingContext.Provider value={isDragging}>
                                    <DragDropContext
                                        onDragEnd={onDragEnd}
                                        onDragStart={onDragStart}
                                        onBeforeDragStart={onBeforeDragStart}
                                        onBeforeCapture={handleOnBeforeCapture}
                                    >
                                        <Preview
                                            ref={containerСRef}
                                            sort={sort}
                                            width={containerWidth}
                                            setSort={setSort}
                                            columns={columns}
                                            preview={preview}
                                            filter={filters}
                                            runId={runId}
                                            changeFilter={handleChangeFilter}
                                            selectedRows={selectedRows}
                                            setSelectedAll={setSelectedAll}
                                            containerRef={containerRef}
                                            handleCheck={handleCheck}
                                            loadData={getPreview}
                                            getContainerWidth={getContainerWidth}
                                            toggleSelection={toggleSelection}
                                            toggleSelectionInGroup={toggleSelectionInGroup}
                                            multiSelectTo={multiSelectTo}
                                            selectedDND={selectedDND}
                                            draggingId={draggingId}
                                        />
                                    </DragDropContext>
                                </IsDraggingContext.Provider>
                            {info && preview && preview.length ? (
                                <Grid
                                    className="grid-footer-panel grid-footer-panel_autogrouping"
                                    style={{ width: containerWidth - 2 }}
                                >
                                    <div className="grid-footer-panel_info">
                                        <div>
                                            <div>
                                                {t('ordersCount') + ':'}
                                                <span className="footer-info-value">
                                                    {info.ordersCount}
                                                </span>
                                            </div>
                                            <div>
                                                {t('palletsCountGroup') + ':'}
                                                <span className="footer-info-value">
                                                    {info.palletsCount}
                                                </span>
                                            </div>
                                            <div>
                                                {t('shippingsCount') + ':'}
                                                <span className="footer-info-value">
                                                    {info.shippingsCount}
                                                </span>
                                            </div>
                                        </div>
                                        <div>
                                            <div>
                                                {t('totalShippingAmount') + ':'}
                                                <span className="footer-info-value">
                                                    {info.totalAmount}
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                    <Grid.Row align="right">
                                        <Grid.Column>
                                            <div>
                                                {/* <Button color="grey" onClick={handleClose}>
                                                {t('CancelButton')}
                                            </Button>*/}
                                                <Button
                                                    color="green"
                                                    loading={applyProgress}
                                                    disabled={
                                                        !isGrouping ||
                                                        !preview.length ||
                                                        !selectedRows.length
                                                    }
                                                    onClick={handleApplyAndSend}
                                                >
                                                    {t('Отправить в ТК')}
                                                </Button>
                                                <Button
                                                    color="blue"
                                                    loading={applyProgress}
                                                    disabled={
                                                        !isGrouping ||
                                                        !preview.length ||
                                                        !selectedRows.length
                                                    }
                                                    onClick={handleApply}
                                                >
                                                    {t('Создать перевозки')}
                                                </Button>
                                            </div>
                                        </Grid.Column>
                                    </Grid.Row>
                                </Grid>
                            ) : null}
                        </Scrollbars>
                    </div>
                </div>
            ) : null}
        </div>
    );
};

export default AutoGrouping;
