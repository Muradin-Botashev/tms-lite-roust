import React, { Component } from 'react';
import { withTranslation } from 'react-i18next';
import { withRouter } from 'react-router-dom';
import { Button, Grid, Loader, Popup, Table } from 'semantic-ui-react';
import InfiniteScrollTable from '../InfiniteScrollTable';
import { debounce } from 'throttle-debounce';
import { PAGE_SIZE } from '../../constants/settings';
import Search from '../Search';
import './style.scss';
import HeaderCellComponent from './components/header-cell';
import BodyCellComponent from './components/body-cell';
import Icon from '../CustomIcon';

const ModalComponent = ({ element, props, children }) => {
    if (!element) {
        return <>{children}</>;
    }
    return React.cloneElement(element(props), props, children);
};

class TableInfo extends Component {
    constructor(props) {
        super(props);

        const { storageFilterItem, storageSortItem } = props;
        window.scrollTo(0, 0);

        this.state = {
            page: 1,
            search: '',
            filters: storageFilterItem
                ? JSON.parse(localStorage.getItem(storageFilterItem)) || {}
                : {},
            sort: storageSortItem ? JSON.parse(localStorage.getItem(storageSortItem)) || {} : {},
            width: {},
        };
    }

    componentDidMount() {
        const { location, history } = this.props;
        const { state } = location;

        if (state) {
            this.setState(
                {
                    search: state.filter && state.filter.search,
                    page: state.skip && state.take ? parseInt(state.skip / state.take) : 1,
                },
                () => {
                    history.replace(location.pathname, null);
                    this.load(false, true, () => {
                        if (state.scroll) this.container.scrollTop = state.scroll;
                    });
                },
            );
        } else {
            this.load();
        }
    }

    mapData = (isConcat, isReload, scrollTop) => {
        const { search, page, filters, sort } = this.state;
        const { name } = this.props;

        const params = {
            filter: {
                filter: {
                    ...filters,
                    search,
                },
                take: isReload ? page * PAGE_SIZE : PAGE_SIZE,
                skip: isReload ? 0 : (page - 1) * PAGE_SIZE,
                sort,
            },
            isConcat,
            name,
            scrollTop
        };

        return params;
    };

    load = (isConcat, isReload, scrollTop) => {
        const { loadList } = this.props;
        loadList(this.mapData(isConcat, isReload, scrollTop));
    };

    nextPage = () => {
        const { totalCount, list = [] } = this.props;
        const {page, width} = this.state;

        if (page === 1) { //запоминаем ширину, чтоб при скролле ячейки не расширялись
            this.props.headerRow &&
            this.props.headerRow.forEach(item => {
                if (!width[item.name] && this[item.name]) {
                    this.setState(prevState => ({
                        width: {
                            ...prevState.width,
                            [item.name]: this[item.name].offsetWidth
                        }
                    }))
                }
            });
        }

        if (list.length < totalCount) {
            this.setState(
                prevState => ({
                    page: prevState.page + 1,
                }),
                () => this.load(true),
            );
        }
    };

    changeFullTextFilter = (e, { value }) => {
        this.setState({ search: value, page: 1 }, this.load);
    };

    setFilter = (e, { name, value }) => {
        this.setState(prevState => {
            let { filters } = prevState;
            if (value !== null && value !== undefined && value !== '') {
                filters = { ...filters, [name]: value };
            } else if (filters[name] !== null && filters[name] !== undefined) {
                filters = Object.assign({}, filters);
                delete filters[name];
            }
            return {
                ...prevState,
                filters,
                page: 1,
            };
        }, this.debounceSetFilterApiAndLoadList);
    };

    setSort = sort => {
        const { storageSortItem } = this.props;

        storageSortItem && localStorage.setItem(storageSortItem, JSON.stringify(sort));

        this.setState(
            {
                sort,
                page: 1,
            },
            this.loadAndResetContainerScroll,
        );
    };

    setFilterApiAndLoadList = () => {
        const { filters } = this.state;

        const filtersJson = JSON.stringify(filters);
        const { storageFilterItem } = this.props;

        storageFilterItem && localStorage.setItem(storageFilterItem, filtersJson);

        this.loadAndResetContainerScroll();
    };

    debounceSetFilterApiAndLoadList = debounce(300, this.setFilterApiAndLoadList);

    loadAndResetContainerScroll = () => {
        this.load();
        if (this.container && this.container.scrollTop) {
            this.container.scrollTop = 0;
        }
    };

    clearFilter = () => {
        const { storageFilterItem } = this.props;
        localStorage.setItem(storageFilterItem, JSON.stringify({}));
        this.setState(
            {
                filters: {},
                page: 1,
            },
            this.loadAndResetContainerScroll,
        );
    };

    headerRowComponent = () => {
        const { filters, sort, width } = this.state;
        const { name } = this.props;

        return (
            <Table.Row>
                {this.props.headerRow &&
                    this.props.headerRow.map((row, index) => (
                        <HeaderCellComponent
                            key={row.name}
                            row={row}
                            index={index}
                            width={width[row.name]}
                            ref={instance => {
                                this[row.name] = instance;
                            }}
                            filters={this.mapData()}
                            sort={sort}
                            gridName={name}
                            setSort={this.setSort}
                            setFilter={this.setFilter}
                        />
                    ))}
                {this.props.isShowActions ? <Table.HeaderCell /> : null}
            </Table.Row>
        );
    };

    importFromExcel = () => {
        this.fileUploader && this.fileUploader.click();
    };

    exportToExcel = () => {
        this.props.exportToExcel && this.props.exportToExcel(this.mapData());
    };

    onFilePicked = e => {
        const file = e.target.files[0];

        const data = new FormData();
        data.append('FileName', file.name);
        data.append('FileContent', new Blob([file], { type: file.type }));
        data.append('FileContentType', file.type);
        this.props.importFromExcel(data, () => this.load(false, true));

        e.target.value = null;
    };

    handleToggleIsActive = (itemID, checked) => {
        this.props.toggleIsActive(itemID, checked, this.load);
    };

    handleRowClick = (e, id) => {
        const { history, cardLink, name } = this.props;

        if (!cardLink) {
            e.stopPropagation();
        } else {
            history.push({
                pathname: cardLink.replace(':name', name).replace(':id', id),
                state: {
                    ...this.mapData().filter,
                    pathname: history.location.pathname,
                    scroll: this.container.scrollTop,
                },
            });
        }
    };

    render() {
        const {
            headerRow,
            className,
            list = [],
            isShowActions,
            actions,
            loading,
            customRowComponent,
            newLink,
            t,
            name,
            modalCard,
            isImportBtn,
            isExportBtn,
            importLoader,
            exportLoader,
            totalCount,
            history,
        } = this.props;

        const { search, filters, page, width } = this.state;

        return (
            <div className={className}>
                <Loader active={loading && !list.length} size="huge" className="table-loader">
                    Loading
                </Loader>
                <Grid className="table-header-menu">
                    <Grid.Row>
                        <Grid.Column width={5} verticalAlign="middle">
                            <span className="table-header-menu_title">{t(name)}</span>
                            <span className="records-counter">
                                {t('totalCount', { count: totalCount })}
                            </span>
                        </Grid.Column>
                        <Grid.Column width={11} textAlign="right">
                            {newLink ? (
                                <Popup
                                    content={t('add_record')}
                                    position="bottom right"
                                    trigger={
                                        <Button
                                            icon="add"
                                            onClick={() => {
                                                history.push({
                                                    pathname: newLink.replace(':name', name),
                                                    state: {
                                                        ...this.mapData().search,
                                                        pathname: history.location.pathname,
                                                    },
                                                });
                                            }}
                                        />
                                    }
                                />
                            ) : null}
                            {isImportBtn ? (
                                <Popup
                                    content={t('importFromExcel')}
                                    position="bottom right"
                                    trigger={
                                        <Button
                                            icon="upload"
                                            loading={importLoader}
                                            onClick={this.importFromExcel}
                                        />
                                    }
                                />
                            ) : null}
                            {isExportBtn ? (
                                <Popup
                                    content={
                                        t('exportToExcel') // todo
                                    }
                                    position="bottom right"
                                    trigger={
                                        <Button
                                            icon="download"
                                            loading={exportLoader}
                                            onClick={this.exportToExcel}
                                        />
                                    }
                                />
                            ) : null}
                            <Popup
                                content={t('reset_filters')}
                                position="bottom right"
                                trigger={
                                    <Button
                                        icon
                                        className={`clear-filter-btn`}
                                        onClick={this.clearFilter}
                                        disabled={!Object.keys(filters).length}
                                    >
                                        <Icon name="clear-filter" />
                                    </Button>
                                }
                            />
                            <Search
                                value={search}
                                className="search-input"
                                onChange={this.changeFullTextFilter}
                            />
                        </Grid.Column>
                    </Grid.Row>
                    <input
                        type="file"
                        ref={instance => {
                            this.fileUploader = instance;
                        }}
                        style={{ display: 'none' }}
                        onChange={this.onFilePicked}
                    />
                </Grid>
                <div
                    className={`scroll-table-container`}
                    ref={instance => {
                        this.container = instance;
                    }}
                >
                    <InfiniteScrollTable
                        className="grid-table table-info"
                        onBottomVisible={this.nextPage}
                        context={this.container}
                        headerRow={this.headerRowComponent()}
                    >
                        <Table.Body>
                            {customRowComponent
                                ? customRowComponent
                                : list &&
                                  list.map((row, i) => (
                                      <ModalComponent
                                          element={modalCard}
                                          props={{ row, loadList: this.load, name }}
                                          key={`modal_${row.id}`}
                                      >
                                          <Table.Row
                                              key={row.id}
                                              onClick={e => this.handleRowClick(e, row.id)}
                                          >
                                              {headerRow.map((column, index) => (
                                                  <BodyCellComponent
                                                      key={`cell_${row.id}_${column.name}`}
                                                      column={column}
                                                      width={width[column.name]}
                                                      value={
                                                          row[column.name] &&
                                                          typeof row[column.name] === 'object' &&
                                                          !Array.isArray(row[column.name])
                                                              ? row[column.name].value
                                                              : row[column.name]
                                                      }
                                                      valueText={
                                                          row[column.name] &&
                                                          typeof row[column.name] === 'object' &&
                                                          !Array.isArray(row[column.name])
                                                              ? row[column.name].name
                                                              : null
                                                      }
                                                      valueTooltip={
                                                          row[column.name] &&
                                                          typeof row[column.name] === 'object' &&
                                                          !Array.isArray(row[column.name])
                                                              ? row[column.name].tooltip
                                                              : null
                                                      }
                                                      id={row.id}
                                                      toggleIsActive={this.handleToggleIsActive}
                                                      indexRow={i}
                                                      indexColumn={index}
                                                      t={t}
                                                  />
                                              ))}
                                              {isShowActions ? (
                                                  <Table.Cell textAlign="center">
                                                      {actions &&
                                                          actions(row, this.load, t).map(
                                                              (action, index) => (
                                                                  <React.Fragment
                                                                      key={`action_${index}`}
                                                                  >
                                                                      {action}
                                                                  </React.Fragment>
                                                              ),
                                                          )}
                                                  </Table.Cell>
                                              ) : null}
                                          </Table.Row>
                                      </ModalComponent>
                                  ))}
                            <div className="table-bottom-loader">
                                <Loader active={loading && list.length} />
                            </div>
                        </Table.Body>
                    </InfiniteScrollTable>
                </div>
            </div>
        );
    }
}

TableInfo.defaultProps = {
    loadList: () => {},
};

export default withTranslation()(withRouter(TableInfo));
