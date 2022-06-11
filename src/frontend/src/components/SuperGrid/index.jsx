import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { withTranslation } from 'react-i18next';

import { debounce } from 'throttle-debounce';
import _ from 'lodash';

import './style.scss';
import Filter from './components/filter';
import HeaderSearchGrid from './components/header';
import InfiniteScrollTable from '../InfiniteScrollTable';

import Result from './components/result';
import { PAGE_SIZE } from '../../constants/settings';
import { Confirm, Loader } from 'semantic-ui-react';
import Footer from './components/footer';
import { withRouter } from 'react-router-dom';
import IdleTimer from 'react-idle-timer';
import getGridDefaultColumnWidth from "../../utils/getGridDefaultColumnWidth";

const initState = () => ({
    page: 1,
    fullText: '',
    selectedRows: new Set(),
    columns: [],
    width: 0,
});

class SuperGrid extends Component {
    constructor(props) {
        super(props);

        window.scrollTo(0, 0);

        this.idleTimer = null;

        this.state = {
            ...initState(),
        };
    }

    changeRepresentation = () => {
        const { columns } = this.props;
        const { width } = this.state;

        const newColumns = columns.map(item => ({
            ...item,
            width: item.width || getGridDefaultColumnWidth(width, columns.length),
        }));

        this.setState({ columns: newColumns }, () => {
            this.loadList(false, false);
        });
    };

    pageLoading = (isConcat, isReload, scrollTop) => {
        const { getRepresentations, name } = this.props;

        getRepresentations({
            key: name,
            callBackFunc: columns => {
                this.setState(
                    {
                        columns: columns,
                    },
                    () => {
                        this.props.autoUpdateStart(this.mapData(isConcat, isReload, scrollTop));
                    },
                );
            },
        });
    };

    handleVisibilityChange = () => {
        const { autoUpdateStop, autoUpdateStart, startConfigUpdate, stopConfigUpdate } = this.props;

        if (document.hidden) {
            autoUpdateStop();
            stopConfigUpdate();
        } else {
            autoUpdateStart(this.mapData(false, true));
            startConfigUpdate();
        }
    };

    componentDidMount() {
        document.addEventListener('visibilitychange', this.handleVisibilityChange);
        this.timer = null;
        const { location, history, startConfigUpdate } = this.props;
        const { state = {} } = location;

        startConfigUpdate();

        const width = this.container.offsetWidth - 65;
        this.setState({ width });

        if (state) {
            this.setState(
                {
                    fullText: state.filter && state.filter.search,
                    page: state.skip && state.take ? parseInt(state.skip / state.take) : 1,
                },
                () => {
                    history.replace(location.pathname, null);
                    this.pageLoading(false, true, () => {
                        if (state.scroll) this.container.scrollTop = state.scroll;
                    });
                },
            );
        } else {
            this.pageLoading();
        }
    }

    componentDidUpdate(prevProps, prevState) {
        const { selectedRows } = this.state;
        const newSelectedRow = new Set(selectedRows);

        if (prevProps.rows !== this.props.rows) {
            const rowsIds = this.props.allIds && this.props.allIds.length ? this.props.allIds : this.props.rows.map(item => item.id);

            for (let item of selectedRows) {
                if (!rowsIds.includes(item)) {
                    newSelectedRow.delete(item);
                }
            }

            this.setSelected(newSelectedRow);
        }

        if (prevState.selectedRows.size && !this.state.selectedRows.size && this.props.allIds) {
            this.props.clearAllIds([]);
        }
    }

    componentWillUnmount() {
        document.removeEventListener('visibilitychange', this.handleVisibilityChange);
    }

    mapData = (isConcat, isReload, scrollTop) => {
        const { columns, page, fullText } = this.state;
        const { extParams, defaultFilter, name } = this.props;

        let filters = {};
        let sort = {};

        columns.forEach(column => {
            filters = {
                ...filters,
                [column.name]: column.filter,
            };

            if (column.sort === true || column.sort === false) {
                sort = {
                    name: column.name,
                    desc: column.sort,
                };
            }
        });

        let params = {
            filter: {
                filter: {
                    ...filters,
                    search: fullText,
                    ...defaultFilter,
                },
                take: isReload ? page * PAGE_SIZE : PAGE_SIZE,
                skip: isReload ? 0 : (page - 1) * PAGE_SIZE,
                sort,
            },
            ...extParams,
            name,
            isConcat,
            scrollTop,
        };

        return params;
    };

    loadList = (isConcat, isReload) => {
        const { autoUpdateStop, autoUpdateStart } = this.props;
        const { selectedRows } = this.state;

        autoUpdateStop();
        autoUpdateStart(this.mapData(isConcat, isReload));

        if (selectedRows.size) {
            this.props.getActions({ name: this.props.name, ids: Array.from(selectedRows) });
        }
    };

    nextPage = () => {
        const { totalCount, rows = [] } = this.props;

        if (rows.length < totalCount) {
            this.setState(
                prevState => ({
                    page: prevState.page + 1,
                }),
                () => this.loadList(true),
            );
        }
    };

    setFilter = (e, { name, value }) => {
        this.setState(prevState => {
            const nextColumns = [...prevState.columns];
            let index = nextColumns.findIndex(item => item.name === name);
            nextColumns[index] = {
                ...nextColumns[index],
                filter: value,
            };

            return {
                ...prevState,
                columns: [...nextColumns],
                page: 1,
                selectedRows: new Set(),
            };
        }, this.debounceSetFilterApiAndLoadList);
    };

    setSort = sort => {
        this.setState(prevState => {
            const nextColumns = prevState.columns.map(column => ({
                ...column,
                sort: null,
            }));
            if (sort) {
                let index = nextColumns.findIndex(item => item.name === sort.name);
                nextColumns[index] = {
                    ...nextColumns[index],
                    sort: sort.desc,
                };
            }

            return {
                ...prevState,
                columns: [...nextColumns],
                page: 1,
            };
        }, this.debounceSetFilterApiAndLoadList);
    };

    setSelected = item => {
        this.setState(
            {
                selectedRows: item,
            },
            () => {
                item && item.size && this.props.getActions({ name: this.props.name, ids: Array.from(item) });
            },
        );
    };

    setSelectedAll = () => {
        const { selectedRows } = this.state;
        const { allIds = [], getAllIds, name } = this.props;
        let newSelectedRows = new Set();

        if (selectedRows.size) {
            newSelectedRows = new Set();
            this.setSelected(newSelectedRows);
        } else if (allIds && allIds.length) {
            newSelectedRows = new Set(allIds);
            this.setSelected(newSelectedRows);
        } else {
            getAllIds({
                name,
                filter: this.mapData().filter,
                callbackSuccess: ids => {
                    newSelectedRows = new Set(ids);
                    this.setSelected(newSelectedRows);
                },
            });
        }
    };

    changeFullTextFilter = (e, { value }) => {
        this.setState({ fullText: value, page: 1 }, this.setFilterApiAndLoadList);
    };

    clearFilters = () => {
        this.setState(prevState => {
            const { columns } = prevState;

            return {
                ...prevState,
                columns: columns.map(item => ({
                    ...item,
                    filter: '',
                })),
                page: 1,
                selectedRows: new Set(),
            };
        }, this.setFilterApiAndLoadList);
    };

    clearSelectedRows = () => {
        this.setState(
            {
                selectedRows: new Set(),
            },
            () => this.loadList(false, true),
        );
    };

    setFilterApiAndLoadList = () => {
        this.editRepresentations();
        this.loadAndResetContainerScroll();
    };

    debounceSetFilterApiAndLoadList = debounce(300, this.setFilterApiAndLoadList);

    loadAndResetContainerScroll = () => {
        this.loadList();
        if (this.container && this.container.scrollTop) {
            this.container.scrollTop = 0;
        }
    };

    resizeColumn = (size, index) => {
        const { columns } = this.state;

        clearTimeout(this.timer);
        this.setState(prevState => {
            const nextColumns = [...prevState.columns];
            nextColumns[index] = {
                ...nextColumns[index],
                width: size.width,
            };
            return {
                columns: nextColumns,
            };
        });

        let sum = 0;

        columns.forEach(item => {
            sum = sum + item.width + columns.length + 50;
        });

        this.timer = setTimeout(() => {
            this.editRepresentations();
        }, 2000);
    };

    editRepresentations = () => {
        const { editRepresentation, representationName, name, getRepresentations } = this.props;
        const { columns } = this.state;

        if (representationName) {
            editRepresentation({
                key: name,
                name: representationName,
                oldName: representationName,
                value: columns,
                callbackSuccess: () => {
                    //getRepresentations({key: name});
                },
            });
        }
    };

    handleGoToCard = (isEdit, id, source) => {
        const { history, cardLink, newLink, name } = this.props;

        history.push({
            pathname: isEdit
                ? cardLink.replace(':name', source).replace(':id', id)
                : newLink.replace(':name', source),
            state: {
                ...this.mapData().filter,
                scroll: this.container.scrollTop,
                pathname: history.location.pathname,
            },
        });
    };

    onAction = e => {};

    onActive = e => {
        console.log('user is active', e);
        console.log('time remaining', this.idleTimer.getRemainingTime());

        const { autoUpdateStart } = this.props;
        autoUpdateStart(this.mapData(false, true));
    };

    onIdle = e => {
        console.log('user is idle');
        console.log('last active', this.idleTimer.getLastActiveTime());

        const { autoUpdateStop } = this.props;
        autoUpdateStop();
    };

    render() {
        const { fullText, selectedRows, columns, width } = this.state;
        const {
            totalCount: count = 0,
            rows = [],
            progress,
            catalogsFromGrid,
            actions,
            isShowActions,
            confirmation = {},
            closeConfirmation = () => {},
            groupActions,
            isCreateBtn,
            extGrid,
            onlyOneCheck,
            checkAllDisabled,
            disabledCheck,
            storageRepresentationItems,
            name,
            representationName,
            t,
        } = this.props;

        return (
            <>
                <IdleTimer
                    ref={ref => {
                        this.idleTimer = ref;
                    }}
                    element={document}
                    onActive={this.onActive}
                    onIdle={this.onIdle}
                    onAction={this.onAction}
                    debounce={250}
                    timeout={1000 * 60 * 5}
                />
                <Loader active={progress && !rows.length} size="huge" className="table-loader">
                    Loading
                </Loader>
                <HeaderSearchGrid
                    isCreateBtn={isCreateBtn}
                    goToCard={this.handleGoToCard}
                    name={name}
                    loadList={this.loadList}
                    width={width}
                    pageLoading={this.changeRepresentation}
                    searchValue={fullText}
                    searchOnChange={this.changeFullTextFilter}
                    counter={count}
                    storageRepresentationItems={storageRepresentationItems}
                    disabledClearFilter={!columns.find(column => column.filter)}
                    representationName={representationName}
                    clearFilter={this.clearFilters}
                    filter={this.mapData()}
                    setSelected={this.setSelected}
                    selectedRows={selectedRows}
                />
                <div
                    className={`scroll-grid-container${extGrid ? ' grid_small' : ''}`}
                    ref={instance => {
                        this.container = instance;
                    }}
                >
                    <InfiniteScrollTable
                        className="grid-table"
                        unstackable
                        celled={false}
                        selectable={false}
                        columns={columns}
                        fixed
                        headerRow={
                            <Filter
                                columns={columns}
                                indeterminate={!!(selectedRows.size && selectedRows.size !== count)}
                                all={!!(selectedRows.size && selectedRows.size === count)}
                                catalogs={catalogsFromGrid}
                                isShowActions={isShowActions}
                                gridName={name}
                                checkAllDisabled={checkAllDisabled || onlyOneCheck}
                                setFilter={this.setFilter}
                                setSort={this.setSort}
                                filters={this.mapData()}
                                setSelectedAll={this.setSelectedAll}
                                resizeColumn={this.resizeColumn}
                            />
                        }
                        context={this.container}
                        onBottomVisible={this.nextPage}
                    >
                        <Result
                            columns={columns}
                            rows={rows}
                            progress={progress}
                            name={name}
                            goToCard={this.handleGoToCard}
                            actions={actions}
                            onlyOneCheck={onlyOneCheck}
                            loadList={this.loadList}
                            disabledCheck={disabledCheck}
                            selectedRows={selectedRows}
                            setSelected={this.setSelected}
                            isShowActions={isShowActions}
                        />
                    </InfiniteScrollTable>
                    {selectedRows.size ? (
                        <Footer
                            gridName={name}
                            groupActions={groupActions}
                            selectedRows={selectedRows}
                            load={this.loadList}
                        />
                    ) : null}
                </div>
                <Confirm
                    dimmer="blurring"
                    open={confirmation.open}
                    onCancel={closeConfirmation}
                    onConfirm={confirmation.onConfirm}
                    cancelButton={t('cancelConfirm')}
                    content={confirmation.content}
                />
            </>
        );
    }
}

SuperGrid.propTypes = {
    totalCount: PropTypes.number,
    columns: PropTypes.array.isRequired,
    rows: PropTypes.array.isRequired,
    progress: PropTypes.bool,
    loadList: PropTypes.func,
    autoUpdateStart: PropTypes.func,
    autoUpdateStop: PropTypes.func,
};
SuperGrid.defaultProps = {
    loadList: () => {},
    autoUpdateStart: () => {},
    autoUpdateStop: () => {},
    confirmation: {},
    closeConfirmation: () => {},
    clearStore: () => {},
    getLookupList: () => {},
    getAllIds: () => {},
    disabledCheck: () => {},
};

export default withTranslation()(withRouter(SuperGrid));
