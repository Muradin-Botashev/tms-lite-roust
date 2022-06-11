import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Button, Form, Grid, Popup, Table } from 'semantic-ui-react';
import Date from '../../components/BaseComponents/Date';
import CheckBox from '../../components/BaseComponents/Checkbox';
import { dateToString } from '../../utils/dateTimeFormater';
import {
    columnsSelector,
    exportProgressSelector,
    getReportRequest,
    progressSelector,
    reportExportToExcelRequest,
    reportSelector,
} from '../../ducks/reports';
import { OPERATIONAL_REPORT_TYPE } from '../../constants/reportType';
import { useTranslation } from 'react-i18next';
import Block from '../../components/CardLayout/components/block';
import CellValue from '../../components/ColumnsValue';
import BodyCell from '../../components/SuperGrid/components/body_cell';
import FacetField from '../../components/FilterComponents';
import { ORDERS_GRID } from '../../constants/grids';
import Icon from '../../components/CustomIcon';
import generate from "@babel/generator";

const OperationalReport = () => {
    const dispatch = useDispatch();
    const { t } = useTranslation();

    let [params, setParams] = useState({
        startDate: dateToString(),
        endDate: dateToString(),
        deliveryType: true,
        client: true,
        daily: false,
    });

    let [isDisabledBtn, setIsDisadledBtn] = useState(true);

    let [filter, setFilter] = useState({});
    let [sort, setSort] = useState({});

    const columns = useSelector(columnsSelector);
    const report = useSelector(reportSelector);
    const exportProgress = useSelector(exportProgressSelector);
    const loading = useSelector(progressSelector);

    useEffect(() => {
        getReport();
    }, [filter, sort]);

    /* useEffect(() => {
        setFilter({});
        setSort({});
    }, [columns])*/

    const mapData = () => {
        return {
            type: OPERATIONAL_REPORT_TYPE,
            params: {
                ...params,
                filter,
                sort,
            },
        };
    };

    const getReport = () => {
        dispatch(getReportRequest(mapData()));
    };

    const handleChangeParams = (e, { name, value }) => {
        const newParams = {
            ...params,
            [name]: value,
        };
        setParams(newParams);

        if (!isDisabledBtn && !newParams.deliveryType && !newParams.client && !newParams.daily) {
            setIsDisadledBtn(true);
        } else {
            setIsDisadledBtn(false);
        }
    };

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

    const clearFilter = () => {
        setFilter({});
    };

    const exportExcel = () => {
        dispatch(reportExportToExcelRequest(mapData()));
    };

    const generateReport = () => {
        setFilter({});
        setSort({});
    };

    const item = {
        menuItem: 'report',
        render: () => (
            <Table>
                <Table.Header>
                    <Table.Row>
                        {columns.map((column, i) => (
                            <Table.HeaderCell>
                                <FacetField
                                    key={'facet' + column.name}
                                    notWrapHeader
                                    index={i}
                                    gridName={ORDERS_GRID}
                                    name={column.name}
                                    displayNameKey={column.displayNameKey}
                                    sort={sort && sort.name === column.name ? sort.desc : null}
                                    setSort={setSort}
                                    type={column.type}
                                    value={filter[column.name]}
                                    filters={filter}
                                    setFilter={handleChangeFilter}
                                    source={column.source}
                                />
                            </Table.HeaderCell>
                        ))}
                    </Table.Row>
                </Table.Header>
                <Table.Body>
                    {report.map((row, indexRow) => (
                        <Table.Row key={row.id}>
                            {columns.map((column, indexColumn) => (
                                <Table.Cell key={`${row.id}_${column.name}`}>
                                    <CellValue
                                        {...column}
                                        indexRow={indexRow}
                                        indexColumn={indexColumn}
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
                                        rowId={row.id}
                                        t={t}
                                    />
                                </Table.Cell>
                            ))}
                        </Table.Row>
                    ))}
                </Table.Body>
            </Table>
        ),
        actions: () => [
            <Popup
                content={t('exportToExcel')}
                position="bottom right"
                trigger={
                    <Button
                        icon="download"
                        disabled={!report.length}
                        loading={exportProgress}
                        onClick={exportExcel}
                    />
                }
            />,
            <Popup
                content={t('reset_filters')}
                position="bottom right"
                trigger={
                    <Button
                        icon
                        className={`clear-filter-btn`}
                        onClick={clearFilter}
                        disabled={!Object.keys(filter).length}
                    >
                        <Icon name="clear-filter" />
                    </Button>
                }
            />,
        ],
    };

    return (
        <div className="container">
            <div className="report">
                <div className="report_params">
                    <Form>
                        <Grid>
                            <Grid.Row columns={2}>
                                <Grid.Column>
                                    <Date
                                        name="startDate"
                                        value={params.startDate}
                                        onChange={handleChangeParams}
                                    />
                                </Grid.Column>
                                <Grid.Column>
                                    <Date
                                        name="endDate"
                                        value={params.endDate}
                                        onChange={handleChangeParams}
                                    />
                                </Grid.Column>
                            </Grid.Row>
                            <Grid.Row columns={4}>
                                <Grid.Column>
                                    <CheckBox
                                        name="deliveryType"
                                        checked={params.deliveryType}
                                        onChange={handleChangeParams}
                                    />
                                </Grid.Column>
                                <Grid.Column>
                                    <CheckBox
                                        name="client"
                                        checked={params.client}
                                        onChange={handleChangeParams}
                                    />
                                </Grid.Column>
                                <Grid.Column>
                                    <CheckBox
                                        name="daily"
                                        checked={params.daily}
                                        onChange={handleChangeParams}
                                    />
                                </Grid.Column>
                                <Grid.Column textAlign="right">
                                    <Button primary disabled={isDisabledBtn} onClick={generateReport}>
                                        {t('Generate report')}
                                    </Button>
                                </Grid.Column>
                            </Grid.Row>
                        </Grid>
                    </Form>
                </div>
                <div className="report_table">
                    <Block item={item} actions={item.actions} loading={loading} isFullScreen />
                </div>
            </div>
        </div>
    );
};

export default OperationalReport;
