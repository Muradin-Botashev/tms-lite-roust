import React, { useState } from 'react';

import { ORDERS_GRID } from '../../../constants/grids';
import { Button, Dropdown, Grid, Icon, Loader, Popup } from 'semantic-ui-react';
import MassChanges from './mass_changes';

import { useTranslation } from 'react-i18next';
import { useSelector } from 'react-redux';
import { infoSelector, progressSelector } from '../../../ducks/gridActions';
import { numbersFormat } from '../../../utils/numbersFormat';
import { autogroupingOrdersSelector } from '../../../ducks/gridList';
import { GRID_AUTO_GROUPING_LINK } from '../../../router/links';
import useReactRouter from 'use-react-router';

const InfoView = ({ info, t, gridName, selectedRowsLen }) => {
    const formatNumber = (value, decimals) => {
        return new Intl.NumberFormat().format(numbersFormat(parseFloat(value).toFixed(decimals)));
    };
    return (
        <div className="footer-info">
            {/*<div className="footer-info-close" onClick={handleClose}>
                <Icon name="sort down" />
            </div>*/}
            {gridName === ORDERS_GRID ? (
                <>
                    <div>
                        <div>
                            {t('orders_selected')}
                            <span className="footer-info-value">{info.count}</span>
                        </div>
                        <div>
                            {t('target_weight')}
                            <span className="footer-info-value">{info.weightKg}</span>
                        </div>
                    </div>
                    <div>
                        <div>
                            {t('number_of_pallets')}
                            <span className="footer-info-value">{info.palletsCount}</span>
                        </div>
                        <div>
                            {t('number_of_boxes')}
                            <span className="footer-info-value">{info.boxesCount}</span>
                        </div>
                    </div>
                    <div>
                        <div>
                            {t('downtime_amount')}
                            <span className="footer-info-value">
                                {formatNumber(info.downtimeAmount, 2)}
                            </span>
                        </div>
                        <div>
                            {t('trucks_downtime')}
                            <span className="footer-info-value">{info.trucksDowntime}</span>
                        </div>
                    </div>
                    <div>
                        <div>
                            {t('delivery_cost')}
                            <span className="footer-info-value">
                                {formatNumber(info.deliveryCost, 2)}
                            </span>
                        </div>
                        <div>
                            {t('total_amount')}
                            <span className="footer-info-value">
                                {formatNumber(info.totalAmount, 2)}
                            </span>
                        </div>
                    </div>
                </>
            ) : (
                <div>
                    {t('shippings_selected')}
                    <span className="footer-info-value">{selectedRowsLen}</span>
                </div>
            )}
        </div>
    );
};

const ToggleSummaryButton = ({ isOpen, toggleOpen }) => {
    return (
        <div className="summary-content-block_footer_accordion" onClick={toggleOpen}>
            <Icon name={isOpen ? 'angle up' : 'angle down'} />
        </div>
    );
};

const Footer = ({ groupActions, load, clearSelectedRows, gridName, selectedRows }) => {
    const { t } = useTranslation();
    const { history } = useReactRouter();
    let [isOpen, setIsOpen] = useState(false);

    const info = useSelector(state => infoSelector(state));
    const progress = useSelector(progressSelector);

    const toggleOpen = () => {
        setIsOpen(!isOpen);
    };
    const isAutoGroupingOrders = useSelector(state => autogroupingOrdersSelector(state, gridName));

    const handleGoToAutoGroup = () => {
        history.push({
            pathname: GRID_AUTO_GROUPING_LINK.replace(':name', gridName),
            state: {
                pathname: history.location.pathname,
                selectedIds: Array.from(selectedRows),
            },
        });
    };

    return (
        <Grid className="grid-footer-panel">
            {isOpen ? (
                <Grid.Row>
                    <Grid.Column width={10}>
                        <InfoView
                            info={info}
                            t={t}
                            gridName={gridName}
                            selectedRowsLen={selectedRows.size}
                        />
                    </Grid.Column>
                </Grid.Row>
            ) : null}
            <Grid.Row>
                <Grid.Column width={8}>
                    <div className="footer_actions">
                        <ToggleSummaryButton isOpen={isOpen} toggleOpen={toggleOpen} />
                        {isAutoGroupingOrders && (
                            <Button
                                className="footer_actions_button"
                                size="mini"
                                compact
                                icon="boxes"
                                onClick={handleGoToAutoGroup}
                            >
                                <Icon name="boxes" />
                                Сформировать перевозки
                            </Button>
                        )}
                        <Loader active={progress} inline="centered" />
                        {groupActions
                            ? groupActions().require.map(action => (
                                  <Button
                                      className="footer_actions_button"
                                      key={action.name}
                                      loading={action.loading}
                                      disabled={action.loading}
                                      size="mini"
                                      compact
                                      onClick={() =>
                                          action.action(action.ids, () => load(false, true))
                                      }
                                  >
                                      <Icon name="circle" color={action.color} />
                                      {action.name}
                                  </Button>
                              ))
                            : null}
                        {groupActions && groupActions().other.length ? (
                            <Dropdown
                                icon="ellipsis horizontal"
                                floating
                                button
                                upward
                                className="icon mini ellipsis-actions-btn"
                            >
                                <Dropdown.Menu>
                                    <Dropdown.Menu scrolling>
                                        {groupActions().order.length &&
                                        groupActions().shipping.length ? (
                                            <>
                                                {groupActions().order.map(action => (
                                                    <Dropdown.Item
                                                        key={action.name}
                                                        text={action.name}
                                                        label={{
                                                            color: action.color,
                                                            empty: true,
                                                            circular: true,
                                                        }}
                                                        onClick={() =>
                                                            action.action(action.ids, () =>
                                                                load(false, true),
                                                            )
                                                        }
                                                    />
                                                ))}
                                                <Dropdown.Divider />
                                                {groupActions().shipping.map(action => (
                                                    <Dropdown.Item
                                                        key={action.name}
                                                        text={action.name}
                                                        label={{
                                                            color: action.color,
                                                            empty: true,
                                                            circular: true,
                                                        }}
                                                        onClick={() =>
                                                            action.action(action.ids, () =>
                                                                load(false, true),
                                                            )
                                                        }
                                                    />
                                                ))}
                                            </>
                                        ) : (
                                            groupActions().other.map(action => (
                                                <Dropdown.Item
                                                    key={action.name}
                                                    text={action.name}
                                                    label={{
                                                        color: action.color,
                                                        empty: true,
                                                        circular: true,
                                                    }}
                                                    onClick={() =>
                                                        action.action(action.ids, () =>
                                                            load(false, true),
                                                        )
                                                    }
                                                />
                                            ))
                                        )}
                                    </Dropdown.Menu>
                                </Dropdown.Menu>
                            </Dropdown>
                        ) : null}
                    </div>
                </Grid.Column>
                <Grid.Column width={8} floated="right">
                    <MassChanges gridName={gridName} load={() => load(false, true)} />
                </Grid.Column>
            </Grid.Row>
        </Grid>
    );
};

export default Footer;
