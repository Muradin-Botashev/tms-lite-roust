import React, { useCallback, useEffect } from 'react';
import { Icon, Tab } from 'semantic-ui-react';
import Route from './route';
import { useDispatch, useSelector } from 'react-redux';
import { getLookupRequest, valuesListSelector } from '../../../../ducks/lookup';

const Routes = ({ form, onChange, routeActiveIndex, tabChange, settings }) => {
    const dispatch = useDispatch();
    const { routePoints: points = [] } = form;
    const stateColors = useSelector(state => valuesListSelector(state, 'vehicleState')) || [];

    useEffect(() => {
        if (!stateColors.length) {
            dispatch(
                getLookupRequest({
                    name: 'vehicleState',
                    isForm: true,
                    isState: true,
                }),
            );
        }
    }, []);

    const handleChange = useCallback(
        (point, index) => {
            const newPoints = [...points];
            newPoints[index] = point;
            onChange(null, {
                name: 'routePoints',
                value: newPoints,
            });
        },
        [points],
    );

    const pointsTabs = [];

    points.forEach((point, i) => {
        const state = stateColors.find(x => x.name === point.vehicleStatus);
        const color = state ? state.color : 'grey';
        pointsTabs.push({
            menuItem: {
                key: i,
                content: (
                    <label>
                        <Icon color={color} name="circle" />
                        {point.warehouseName}
                    </label>
                ),
            },
            render: () => {
                return (
                    <Route
                        index={i}
                        form={form}
                        point={point}
                        settings={settings}
                        pointChange={handleChange}
                        onChange={onChange}
                    />
                );
            },
        });
    });

    return (
        <div className="tabs-card">
            <Tab
                className="all-tabs"
                panes={pointsTabs}
                activeIndex={routeActiveIndex}
                menu={{ vertical: true }}
                menuPosition="left"
                onTabChange={tabChange}
            />
        </div>
    );
};

export default Routes;
